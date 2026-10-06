using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
if (args.Contains("--self-test")) { SelfTests.Run(); return; }
if (args.Length == 2 && args[0] == "--replay-fixtures") { ReplayFixtures.Verify(args[1]); return; }
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://127.0.0.1:5080");
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 8 * 1024 * 1024);
builder.Services.AddRateLimiter(o => {
    o.RejectionStatusCode = 429;
    o.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 90, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true }));
});
builder.Services.AddHttpClient("receipt", c => c.Timeout = TimeSpan.FromSeconds(12));
var mysql = Environment.GetEnvironmentVariable("PRISMA_MYSQL") ?? throw new InvalidOperationException("Set PRISMA_MYSQL to your MySQL connection string. Never put it in the mobile app.");
using var db = new LeagueStore(mysql);
if (args.Length == 2 && args[0] == "--import-sqlite") { db.ImportSqlite(args[1]); Console.WriteLine("V4 identities, records and backups imported into MySQL."); return; }
var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(async context => {
    context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(new { error = "Sunucu işlemi tamamlanamadı", traceId = context.TraceIdentifier });
}));
app.UseRateLimiter();
app.Use(async (context, next) => { context.Response.Headers.CacheControl = "no-store"; context.Response.Headers.XContentTypeOptions = "nosniff"; await next(context); });
app.MapGet("/health", () => Results.Ok(new { status = "ok", mode = "endless-real-player-league", database = "mysql", replayVersion = 14 }));
app.MapPost("/api/players", (NameInput input) => {
    if (input.Name is null || !Regex.IsMatch(input.Name, @"^[\p{L}][\p{L} ]{1,23}( [\p{L}]\.)?$") || input.Name.Length > 28) return Results.BadRequest(new { error = "Geçerli bir görünen ad gerekli." });
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    var player = new Player { Id = Guid.NewGuid().ToString("N"), Name = "Oyuncu " + RandomNumberGenerator.GetInt32(100000, 1000000), TokenHash = LeagueStore.Hash(token) };
    lock (db.Gate) { db.Data.Players.Add(player); db.Save(); }
    return Results.Ok(new { player.Id, token });
}).RequireRateLimiting("api");
app.MapGet("/api/ranks", (HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    lock (db.Gate) {
        var rows = db.Ranks(); var me = rows.FirstOrDefault(r => r.Id == player.Id);
        var rival = me is null ? rows.LastOrDefault() : rows.LastOrDefault(r => r.Position < me.Position);
        return Results.Ok(new { rows = rows.Take(100), me, rival });
    }
}).RequireRateLimiting("api");
Progression.Map(app,db);
app.MapPost("/api/runs", (StartInput input, HttpRequest req) => {
    var mode=input.Mode ?? "endless";
    if(mode is not ("endless" or "daily" or "crystals" or "survival")) return Results.BadRequest();
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    lock (db.Gate) {
        db.Data.Tickets.RemoveAll(t => t.Created < DateTimeOffset.UtcNow.AddDays(-1));
        if (db.Data.Tickets.Count(t => t.PlayerId == player.Id && t.Result is null) >= 100) return Results.StatusCode(429);
        var ticket = new Ticket { Id = Guid.NewGuid().ToString("N"), PlayerId = player.Id, ReplayVersion = 14, Seed = mode == "daily" ? Progression.DailySeed(Progression.Day(DateTimeOffset.UtcNow)) : RandomNumberGenerator.GetInt32(1, int.MaxValue), Created = DateTimeOffset.UtcNow };
        db.Data.Tickets.Add(ticket);
        db.Data.Extras.Runs[ticket.Id]=new(mode,Progression.Day(ticket.Created),Progression.Week(ticket.Created));
        db.Save(); return Results.Ok(new { ticket.Id, ticket.Seed, day=Progression.Day(ticket.Created), week=Progression.Week(ticket.Created), replayVersion = 14 });
    }
}).RequireRateLimiting("api");
app.MapPost("/api/runs/{id}/finish", (string id, FinishInput input, HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    Ticket? ticket;
    lock (db.Gate) {
        ticket = db.Data.Tickets.FirstOrDefault(t => t.Id == id && t.PlayerId == player.Id);
        if (ticket is null) return Results.NotFound();
        if (ticket.Result is not null) { Progression.Award(db,ticket,ticket.Result); db.Save(); return Results.Ok(ticket.Result); }
        if (ticket.Created < DateTimeOffset.UtcNow.AddDays(-1)) return Results.BadRequest(new { error = "Koşu kaydı süresi doldu." });
    }
    if (input.Ticks / 60.0 > (DateTimeOffset.UtcNow - ticket.Created).TotalSeconds + 3) return Results.BadRequest(new { error = "Geçersiz koşu süresi." });
    var mode = db.Data.Extras.Runs.GetValueOrDefault(ticket.Id)?.Mode ?? "endless";
    var result = ReplayEngine.Verify(ticket.Seed, input, ticket.ReplayVersion, mode);
    if (result is null) return Results.BadRequest(new { error = "Koşu doğrulanamadı." });
    lock (db.Gate) {
        if (ticket.Result is not null) { Progression.Award(db,ticket,ticket.Result); db.Save(); return Results.Ok(ticket.Result); }
        ticket.Result = result;
        Progression.Award(db,ticket,result);
        if (mode == "endless" && result.Score > player.Score) { player.Score = result.Score; player.RecordAt = DateTimeOffset.UtcNow; }
        db.Save();
    }
    return Results.Ok(result);
}).RequireRateLimiting("api");
app.MapPost("/api/purchases/verify", async (PurchaseInput input, HttpRequest req, IHttpClientFactory clients) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    if (input.ProductId is not ("prisma_remove_ads" or "prisma_prism_pack" or "prisma_patterns_pack" or "prisma_trails_pack") || string.IsNullOrWhiteSpace(input.Receipt) || input.Receipt.Length > 1000000 || input.Source is not ("google_play" or "app_store")) return Results.BadRequest();
    var endpoint = Environment.GetEnvironmentVariable("RECEIPT_VALIDATOR_URL");
    var secret = Environment.GetEnvironmentVariable("RECEIPT_VALIDATOR_SECRET");
    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https" || string.IsNullOrWhiteSpace(secret)) return Results.StatusCode(503);
    try {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = JsonContent.Create(input) };
        request.Headers.Authorization = new("Bearer", secret);
        using var response = await clients.CreateClient("receipt").SendAsync(request);
        if (!response.IsSuccessStatusCode) return Results.StatusCode(502);
        var verified = await response.Content.ReadFromJsonAsync<ReceiptResult>();
        if (verified is null || !verified.Valid || verified.ProductId != input.ProductId || string.IsNullOrWhiteSpace(verified.TransactionId)) return Results.BadRequest(new { valid = false });
        var key = input.Source + ":" + verified.TransactionId;
        lock (db.Gate) {
            if (db.Data.Transactions.TryGetValue(key, out var owner) && owner != player.Id) return Results.Conflict(new { valid = false });
            db.Data.Transactions[key] = player.Id; player.Entitlements.Add(input.ProductId); db.Save();
        }
        return Results.Ok(new { valid = true });
    } catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException) { return Results.StatusCode(502); }
}).RequireRateLimiting("api");
app.MapGet("/api/entitlements", (HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    lock (db.Gate) return Results.Ok(player.Entitlements.ToArray());
}).RequireRateLimiting("api");
app.MapPut("/api/backup", (BackupInput input, HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    if (input.Snapshot is null || input.Snapshot.Length > 200000) return Results.BadRequest();
    try {
        using var doc = JsonDocument.Parse(input.Snapshot);
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return Results.BadRequest();
    } catch (JsonException) { return Results.BadRequest(); }
    lock (db.Gate) { player.Backup = input.Snapshot; db.Save(); }
    return Results.Ok(new { saved = true });
}).RequireRateLimiting("api");
app.MapGet("/api/backup", (HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    lock (db.Gate) return Results.Ok(new { snapshot = player.Backup });
}).RequireRateLimiting("api");
app.MapDelete("/api/account", (HttpRequest req) => {
    var player = db.Auth(req); if (player is null) return Results.Unauthorized();
    lock (db.Gate) {
        var ids = db.Data.Tickets.Where(t => t.PlayerId == player.Id).Select(t => t.Id)
            .Concat(db.Data.Extras.Scores.Where(s => s.PlayerId == player.Id).Select(s => s.RunId)).ToHashSet();
        db.Data.Tickets.RemoveAll(t => t.PlayerId == player.Id);
        db.Data.Extras.Scores.RemoveAll(s => s.PlayerId == player.Id);
        db.Data.Extras.Events.RemoveAll(e => e.PlayerId == player.Id);
        foreach (var id in ids) { db.Data.Extras.Runs.Remove(id); db.Data.Extras.Reviews.Remove(id); }
        db.Data.Extras.Wallets.Remove(player.Id);
        db.Data.Extras.Recovery.Remove(player.Id);
        db.Data.Extras.BaselineScores.Remove(player.Id);
        foreach (var key in db.Data.Transactions.Where(t => t.Value == player.Id).Select(t => t.Key).ToArray())
            db.Data.Transactions.Remove(key);
        db.Data.Players.RemoveAll(p => p.Id == player.Id);
        db.Save();
    }
    return Results.Ok(new { deleted = true });
}).RequireRateLimiting("api");
app.Run();
public record NameInput(string Name);
public record PurchaseInput(string ProductId, string? PurchaseId, string Source, string Receipt);
public record ReceiptResult(bool Valid, string ProductId, string TransactionId);

public record BackupInput(string Snapshot);
