using System.Security.Cryptography;
using MySqlConnector;

public sealed class ExtraState {
    public Dictionary<string, RunMeta> Runs { get; set; } = [];
    public Dictionary<string, Wallet> Wallets { get; set; } = [];
    public Dictionary<string, string> Recovery { get; set; } = [];
    public List<PeriodScore> Scores { get; set; } = [];
    public List<TelemetryEvent> Events { get; set; } = [];
    public Dictionary<string, int> Prices { get; set; } = new() { ["rose"] = 350, ["solar"] = 650, ["violet"] = 1000, ["ice"] = 1600 };
    public List<LiveTask> Tasks { get; set; } = [new("crystals", "Kristal avcısı", 40, 80)];
    public List<LiveEvent> LiveEvents { get; set; } = [];
    public Dictionary<string, int> BaselineScores { get; set; } = [];
    public Dictionary<string, string> Reviews { get; set; } = [];
}
public sealed class Wallet {
    public int Balance { get; set; }
    public List<CoinEntry> Ledger { get; set; } = [];
    public HashSet<string> Owned { get; set; } = ["aqua"];
    public bool Apply(string key, int amount, string reason) {
        if (Ledger.Any(e => e.Key == key)) return false;
        if ((long)Balance + amount < 0 || (long)Balance + amount > 10000000) throw new InvalidOperationException("Invalid balance");
        Balance += amount;
        Ledger.Add(new(key, amount, reason, DateTimeOffset.UtcNow, Balance)); return true;
    }
}
public record CoinEntry(string Key, int Amount, string Reason, DateTimeOffset At, int Balance);
public record RunMeta(string Mode, string Day, string Week);
public record PeriodScore(string PlayerId, string Mode, string Period, string RunId, int Score, DateTimeOffset At);
public record TelemetryEvent(string PlayerId, string Kind, string Device, int Stage, int Hits, int Tick, DateTimeOffset At, int Piece=0,int World=0,int Distance=0,string Cause="unknown");
public record TelemetryInput(string Kind, string Device, int Stage, int Hits,int Tick,int Piece=0,int World=0,int Distance=0,string Cause="unknown");
public record StartInput(string? Mode);
public record RecoveryInput(string Code);
public record CosmeticInput(string Id);
public record AdminConfig(Dictionary<string, int> Prices, LiveTask[] Tasks, LiveEvent[] Events);
public record LiveTask(string Id, string Name, int Target, int Reward);
public record LiveEvent(string Id, string Name, string Day);
public record ReviewInput(string Status);
public static class Progression {
    public static string Day(DateTimeOffset now) => now.ToOffset(TimeSpan.FromHours(3)).ToString("yyyy-MM-dd");
    public static string Week(DateTimeOffset now) { var date = now.ToOffset(TimeSpan.FromHours(3)).Date; return date.AddDays(-((int)date.DayOfWeek + 6) % 7).ToString("yyyy-MM-dd"); }
    public static int DailySeed(string day) { long hash = 17; foreach (var c in day) hash = (hash * 31 + c) & 0x7fffffff; return hash == 0 ? 1 : (int)hash; }
    public static Wallet Wallet(LeagueStore db, string player) { if (!db.Data.Extras.Wallets.TryGetValue(player, out var wallet)) db.Data.Extras.Wallets[player] = wallet = new(); return wallet; }
    public static void Award(LeagueStore db, Ticket ticket, ReplayResult result) {
        if (!db.Data.Extras.Runs.TryGetValue(ticket.Id, out var meta)) return;
        if (!db.Data.Extras.BaselineScores.ContainsKey(ticket.PlayerId)) db.Data.Extras.BaselineScores[ticket.PlayerId]=db.Data.Players.First(p=>p.Id==ticket.PlayerId).Score;
        Wallet(db, ticket.PlayerId).Apply("run:" + ticket.Id, result.Crystals / 2, "Verified run");
        if (db.Data.Extras.Scores.Any(s => s.RunId == ticket.Id)) return;
        db.Data.Extras.Scores.Add(new(ticket.PlayerId, meta.Mode, meta.Mode == "daily" ? meta.Day : meta.Week, ticket.Id,
            meta.Mode == "survival" ? (int)result.Distance : result.Score, DateTimeOffset.UtcNow));
        if (meta.Mode == "endless") db.Data.Extras.Scores.Add(new(ticket.PlayerId, "weekly", meta.Week, ticket.Id, result.Score, DateTimeOffset.UtcNow));
    }
    public static bool Admin(HttpRequest req) {
        var secret = Environment.GetEnvironmentVariable("PRISMA_ADMIN_TOKEN");
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32) return false;
        var supplied = req.Headers["X-Admin-Token"].ToString();
        return CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(supplied), System.Text.Encoding.UTF8.GetBytes(secret));
    }
    public static void Map(WebApplication app, LeagueStore db) {
        app.MapGet("/api/period-ranks", (string mode, HttpRequest req) => {
            var player = db.Auth(req); if (player is null) return Results.Unauthorized();
            if (mode is not ("daily" or "weekly" or "crystals" or "survival")) return Results.BadRequest();
            lock(db.Gate) {
                var period = mode == "daily" ? Day(DateTimeOffset.UtcNow) : Week(DateTimeOffset.UtcNow);
                var rows = db.Data.Extras.Scores.Where(s => s.Mode == mode && s.Period == period && db.Data.Extras.Reviews.GetValueOrDefault(s.RunId) != "rejected")
                    .GroupBy(s => s.PlayerId).Select(g => g.OrderByDescending(s => s.Score).ThenBy(s => s.At).First())
                    .OrderByDescending(s => s.Score).ThenBy(s => s.At).ThenBy(s => s.PlayerId, StringComparer.Ordinal)
                    .Select((s, i) => new RankRow(s.PlayerId, db.Data.Players.First(p => p.Id == s.PlayerId).Name, s.Score, i + 1)).ToArray();
                return Results.Ok(new { period, rows = rows.Take(100), me = rows.FirstOrDefault(r => r.Id == player.Id) });
            }
        }).RequireRateLimiting("api");
        app.MapGet("/api/wallet", (HttpRequest req) => { var p = db.Auth(req); if(p is null) return Results.Unauthorized(); lock(db.Gate) return Results.Ok(Wallet(db,p.Id)); }).RequireRateLimiting("api");
        app.MapPost("/api/cosmetics/buy", (CosmeticInput input, HttpRequest req) => {
            var p = db.Auth(req); if(p is null) return Results.Unauthorized();
            lock(db.Gate) {
                if (!db.Data.Extras.Prices.TryGetValue(input.Id, out var price)) return Results.BadRequest();
                var wallet = Wallet(db,p.Id);
                if(wallet.Owned.Contains(input.Id)) return Results.Ok(wallet);
                if(wallet.Balance < price) return Results.Conflict(new { error = "Insufficient verified coins" });
                wallet.Apply("buy:" + input.Id, -price, "Cosmetic purchase"); wallet.Owned.Add(input.Id); db.Save(); return Results.Ok(wallet);
            }
        }).RequireRateLimiting("api");
        app.MapPost("/api/account/recovery", (HttpRequest req) => {
            var p=db.Auth(req); if(p is null) return Results.Unauthorized();
            var code="PKR1." + Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            lock(db.Gate) { db.Data.Extras.Recovery[p.Id]=LeagueStore.Hash(code); db.Save(); }
            return Results.Ok(new { code });
        }).RequireRateLimiting("api");
        app.MapPost("/api/account/recover", (RecoveryInput input) => {
            if(input.Code is null || input.Code.Length != 69 || !input.Code.StartsWith("PKR1.")) return Results.BadRequest();
            lock(db.Gate) {
                var hash=LeagueStore.Hash(input.Code); var id=db.Data.Extras.Recovery.FirstOrDefault(p=>p.Value==hash).Key;
                var p=db.Data.Players.FirstOrDefault(p=>p.Id==id); if(p is null) return Results.Unauthorized();
                var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); p.TokenHash=LeagueStore.Hash(token);
                db.Data.Extras.Recovery.Remove(p.Id); db.Save(); return Results.Ok(new { token, snapshot=p.Backup });
            }
        }).RequireRateLimiting("api");
        app.MapPost("/api/account/rotate", (HttpRequest req) => {
            var p=db.Auth(req); if(p is null) return Results.Unauthorized();
            var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            lock(db.Gate) { p.TokenHash=LeagueStore.Hash(token); db.Save(); } return Results.Ok(new { token });
        }).RequireRateLimiting("api");
        app.MapPost("/api/telemetry", (TelemetryInput input, HttpRequest req) => {
            var p=db.Auth(req); if(p is null) return Results.Unauthorized();
            if(input.Piece<0 || input.Piece>19 || input.World<0 || input.World>13 || input.Distance<0 || input.Distance>7000000 || input.Cause is not ("unknown" or "fall" or "obstacle")) return Results.BadRequest();
            if(input.Kind is not ("hit" or "quit" or "ad_return" or "run_end" or "error") || input.Device is null || input.Device.Length>80 || input.Stage<0 || input.Stage>10000 || input.Hits<0 || input.Hits>3 || input.Tick<0 || input.Tick>216000) return Results.BadRequest();
            lock(db.Gate) { db.Data.Extras.Events.Add(new(p.Id,input.Kind,input.Device,input.Stage,input.Hits,input.Tick,DateTimeOffset.UtcNow,input.Piece,input.World,input.Distance,input.Cause));
                db.Data.Extras.Events.RemoveAll(e=>e.At<DateTimeOffset.UtcNow.AddDays(-30)); if(db.Data.Extras.Events.Count>20000) db.Data.Extras.Events.RemoveRange(0,db.Data.Extras.Events.Count-20000); db.Save(); }
            return Results.Ok(new { saved=true });
        }).RequireRateLimiting("api");
        app.MapPost("/api/tasks/{id}/claim", (string id,HttpRequest req) => {
            var p=db.Auth(req); if(p is null) return Results.Unauthorized();
            lock(db.Gate) {
                var task=db.Data.Extras.Tasks.FirstOrDefault(t=>t.Id==id); if(task is null) return Results.NotFound();
                var day=Day(DateTimeOffset.UtcNow);
                var crystals=db.Data.Tickets.Where(t=>t.PlayerId==p.Id && t.Result is not null && db.Data.Extras.Runs.GetValueOrDefault(t.Id)?.Day==day).Sum(t=>t.Result!.Crystals);
                if(crystals<task.Target) return Results.Conflict(new { error="Task incomplete" });
                Wallet(db,p.Id).Apply("task:"+day+":"+id,task.Reward,"Daily live task"); db.Save(); return Results.Ok(Wallet(db,p.Id));
            }
        }).RequireRateLimiting("api");
        app.MapGet("/api/config", () => { lock(db.Gate) return Results.Ok(new { prices=db.Data.Extras.Prices,tasks=db.Data.Extras.Tasks,events=db.Data.Extras.LiveEvents }); }).RequireRateLimiting("api");
        app.MapGet("/admin", () => Results.Content(AdminHtml.Page,"text/html; charset=utf-8"));
        app.MapGet("/api/admin/state", (HttpRequest req) => {
            if(!Admin(req)) return Results.Unauthorized(); lock(db.Gate) return Results.Ok(new { prices=db.Data.Extras.Prices,tasks=db.Data.Extras.Tasks,events=db.Data.Extras.LiveEvents,
                telemetrySummary=db.Data.Extras.Events.GroupBy(e=>new {e.World,e.Piece}).Select(g=>new {g.Key.World,g.Key.Piece,hits=g.Count(e=>e.Kind=="hit"),quits=g.Count(e=>e.Kind=="quit"),ends=g.Count(e=>e.Kind=="run_end"),averageExitMeters=g.Where(e=>e.Kind=="quit" || e.Kind=="run_end").Select(e=>(double)e.Distance).DefaultIfEmpty(0).Average()}),
                runs=db.Data.Extras.Scores.TakeLast(100),telemetry=db.Data.Extras.Events.TakeLast(200),reviews=db.Data.Extras.Reviews });
        }).RequireRateLimiting("api");
        app.MapPut("/api/admin/config", (AdminConfig config,HttpRequest req) => {
            if(!Admin(req)) return Results.Unauthorized();
            if(config.Prices is null || config.Tasks is null || config.Events is null || config.Prices.Count>100 || config.Prices.Any(p=>p.Key.Length>40 || p.Value<0 || p.Value>100000) || config.Tasks.Length>20 || config.Tasks.Any(t=>t is null || t.Id is null || t.Name is null || t.Id.Length>40 || t.Name.Length>80 || t.Target<1 || t.Reward<0 || t.Reward>10000) || config.Events.Length>20 || config.Events.Any(e=>e is null || e.Id is null || e.Name is null || e.Id.Length>40 || e.Name.Length>80 || !DateOnly.TryParseExact(e.Day,"yyyy-MM-dd",out _))) return Results.BadRequest();
            lock(db.Gate) { db.Data.Extras.Prices=config.Prices; db.Data.Extras.Tasks=config.Tasks.ToList(); db.Data.Extras.LiveEvents=config.Events.ToList(); db.Save(); } return Results.Ok(new { saved=true });
        }).RequireRateLimiting("api");
        app.MapPut("/api/admin/runs/{id}", (string id, ReviewInput input,HttpRequest req) => {
            if(!Admin(req)) return Results.Unauthorized(); if(input.Status is not ("approved" or "rejected" or "pending")) return Results.BadRequest();
            lock(db.Gate) { if(!db.Data.Extras.Scores.Any(s=>s.RunId==id)) return Results.NotFound(); db.Data.Extras.Reviews[id]=input.Status;
                var owner=db.Data.Extras.Scores.First(s=>s.RunId==id).PlayerId;
                var player=db.Data.Players.First(p=>p.Id==owner);
                player.Score=Math.Max(db.Data.Extras.BaselineScores.GetValueOrDefault(owner),db.Data.Extras.Scores.Where(s=>s.PlayerId==owner && s.Mode=="endless" && db.Data.Extras.Reviews.GetValueOrDefault(s.RunId)!="rejected").Select(s=>s.Score).DefaultIfEmpty(0).Max());
                db.Save(); } return Results.Ok(new { saved=true });
        }).RequireRateLimiting("api");
        app.MapGet("/health/ready", () => { try { using var c=new MySqlConnection(Environment.GetEnvironmentVariable("PRISMA_MYSQL")); c.Open(); return Results.Ok(new { database="ok" }); } catch { return Results.StatusCode(503); } });
    }
}
