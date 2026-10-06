using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MySqlConnector;
using Microsoft.Data.Sqlite;

public sealed class Player {
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string TokenHash { get; set; } = "";
    public string? Backup { get; set; }
    public int Score { get; set; }
    public int LegacyScore { get; set; }
    public DateTimeOffset RecordAt { get; set; } = DateTimeOffset.MaxValue;
    public HashSet<string> Entitlements { get; set; } = [];
}
public sealed class Ticket {
    public string Id { get; set; } = "";
    public string PlayerId { get; set; } = "";
    public int Seed { get; set; }
    public int ReplayVersion { get; set; } = 4;
    public DateTimeOffset Created { get; set; }
    public ReplayResult? Result { get; set; }
}
public sealed class LeagueData {
    public ExtraState Extras { get; set; } = new();
    public int ReplayVersion { get; set; } = 4;
    public List<Player> Players { get; set; } = [];
    public List<Ticket> Tickets { get; set; } = [];
    public Dictionary<string, string> Transactions { get; set; } = [];
}
public record RankRow(string Id, string Name, int Score, int Position);
public sealed class LeagueStore : IDisposable {
    private readonly string connectionString;
    private readonly MySqlConnection lease;
    public readonly object Gate = new();
    public LeagueData Data { get; private set; } = new();
    private Dictionary<string,string> savedPlayers = [], savedTickets = [];
    private Dictionary<string,string> savedTransactions = [];
    private string savedExtras = "{}";
    public LeagueStore(string value) {
        connectionString = value;
        lease = Open();
        using var acquire = lease.CreateCommand();
        acquire.CommandText = "SELECT GET_LOCK(CONCAT('prisma:', DATABASE()),0)";
        if (Convert.ToInt32(acquire.ExecuteScalar()) != 1) throw new InvalidOperationException("This database already has a running Prisma server. Use one API instance.");
        foreach (var sql in Schema.Statements) {
            using var command = lease.CreateCommand(); command.CommandText = sql; command.ExecuteNonQuery();
        }
        using (var check = lease.CreateCommand()) {
            check.CommandText = "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='tickets' AND COLUMN_NAME='replay_version'";
            if (Convert.ToInt32(check.ExecuteScalar()) == 0) {
                using var migrate = lease.CreateCommand();
                migrate.CommandText = "ALTER TABLE tickets ADD COLUMN replay_version INT NOT NULL DEFAULT 4";
                migrate.ExecuteNonQuery();
            }
        }
        using (var command = lease.CreateCommand()) {
            command.CommandText = "SELECT id,name,token_hash,score,legacy_score,record_at,entitlements,backup FROM players ORDER BY id";
            using var reader = command.ExecuteReader();
            while (reader.Read()) Data.Players.Add(new() {
                Id = reader.GetString(0), Name = reader.GetString(1), TokenHash = reader.GetString(2),
                Score = reader.GetInt32(3), LegacyScore = reader.GetInt32(4), RecordAt = DateTimeOffset.Parse(reader.GetString(5), System.Globalization.CultureInfo.InvariantCulture),
                Entitlements = JsonSerializer.Deserialize<HashSet<string>>(reader.GetString(6)) ?? [],
                Backup = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        using (var command = lease.CreateCommand()) {
            command.CommandText = "SELECT id,player_id,seed,created,result,replay_version FROM tickets ORDER BY id";
            using var reader = command.ExecuteReader();
            while (reader.Read()) Data.Tickets.Add(new() { Id = reader.GetString(0), PlayerId = reader.GetString(1), Seed = reader.GetInt32(2),
                Created = DateTimeOffset.Parse(reader.GetString(3), System.Globalization.CultureInfo.InvariantCulture),
                Result = reader.IsDBNull(4) ? null : JsonSerializer.Deserialize<ReplayResult>(reader.GetString(4)), ReplayVersion = reader.GetInt32(5) });
        }
        using (var command = lease.CreateCommand()) {
            command.CommandText = "SELECT transaction_key,player_id FROM purchases";
            using var reader = command.ExecuteReader();
            while (reader.Read()) Data.Transactions.Add(reader.GetString(0), reader.GetString(1));
        }
        using (var command = lease.CreateCommand()) {
            command.CommandText = "SELECT payload FROM progression WHERE id=1";
            if (command.ExecuteScalar() is string payload) Data.Extras = JsonSerializer.Deserialize<ExtraState>(payload) ?? new();
        }
        Remember();
    }
    private MySqlConnection Open() { var c = new MySqlConnection(connectionString); c.Open(); return c; }
    private void Remember() {
        savedPlayers = Data.Players.ToDictionary(p => p.Id, p => JsonSerializer.Serialize(p));
        savedTickets = Data.Tickets.ToDictionary(p => p.Id, p => JsonSerializer.Serialize(p));
        savedTransactions = new(Data.Transactions);
        savedExtras = JsonSerializer.Serialize(Data.Extras);
    }
    public void ImportSqlite(string file) {
        if (Data.Players.Count != 0 || Data.Tickets.Count != 0 || Data.Transactions.Count != 0) throw new InvalidOperationException("Import requires an empty MySQL database.");
        using var sqlite = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(file), Mode = SqliteOpenMode.ReadOnly }.ToString());
        sqlite.Open(); using var command = sqlite.CreateCommand(); command.CommandText = "SELECT payload FROM league WHERE id=1";
        var raw = command.ExecuteScalar() as string ?? throw new InvalidDataException("No V4 league data found");
        var imported = JsonSerializer.Deserialize<LeagueData>(raw) ?? throw new InvalidDataException("Invalid V4 league");
        if (imported.ReplayVersion != 4) throw new InvalidDataException("Only V4 replay data can be imported");
        Data = imported; Save();
    }
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    public Player? Auth(HttpRequest request) {
        var header = request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.Ordinal) || header.Length > 160) return null;
        var hash = Hash(header[7..]);
        lock (Gate) return Data.Players.FirstOrDefault(p => p.TokenHash == hash);
    }
    public void Dispose() => lease.Dispose();
    public void Save() {
        try {
        if (!lease.Ping()) throw new IOException("Database ownership connection lost; restart the API.");
        using var connection = Open(); using var transaction = connection.BeginTransaction();
        void Execute(string sql, params (string,object?)[] parameters) {
            using var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = sql;
            foreach (var (key,value) in parameters) command.Parameters.AddWithValue(key,value ?? DBNull.Value);
            command.ExecuteNonQuery();
        }
        foreach (var p in Data.Players) {
            if (savedPlayers.GetValueOrDefault(p.Id) == JsonSerializer.Serialize(p)) continue;
            Execute("INSERT INTO players(id,name,token_hash,score,legacy_score,record_at,entitlements,backup) VALUES(@id,@name,@hash,@score,@legacy,@date,@rights,@backup) ON DUPLICATE KEY UPDATE name=VALUES(name),token_hash=VALUES(token_hash),score=VALUES(score),legacy_score=VALUES(legacy_score),record_at=VALUES(record_at),entitlements=VALUES(entitlements),backup=VALUES(backup)",
                ("@id",p.Id),("@name",p.Name),("@hash",p.TokenHash),("@score",p.Score),("@legacy",p.LegacyScore),("@date",p.RecordAt.ToString("O")),("@rights",JsonSerializer.Serialize(p.Entitlements)),("@backup",p.Backup));
        }
        var ticketIds = Data.Tickets.Select(t=>t.Id).ToHashSet();
        foreach (var id in savedTickets.Keys.Where(id=>!ticketIds.Contains(id))) Execute("DELETE FROM tickets WHERE id=@id",("@id",id));
        foreach (var t in Data.Tickets) {
            if (savedTickets.GetValueOrDefault(t.Id) == JsonSerializer.Serialize(t)) continue;
            Execute("INSERT INTO tickets(id,player_id,seed,created,result,replay_version) VALUES(@id,@player,@seed,@created,@result,@version) ON DUPLICATE KEY UPDATE result=VALUES(result)",
                ("@id",t.Id),("@player",t.PlayerId),("@seed",t.Seed),("@created",t.Created.ToString("O")),("@result",t.Result is null ? null : JsonSerializer.Serialize(t.Result)),("@version",t.ReplayVersion));
        }
        foreach (var t in Data.Transactions) {
            if (savedTransactions.GetValueOrDefault(t.Key) == t.Value) continue;
            Execute("INSERT INTO purchases(transaction_key,player_id) VALUES(@key,@player) ON DUPLICATE KEY UPDATE player_id=VALUES(player_id)",("@key",t.Key),("@player",t.Value));
        }
        foreach (var key in savedTransactions.Keys.Where(key => !Data.Transactions.ContainsKey(key)))
            Execute("DELETE FROM purchases WHERE transaction_key=@key", ("@key", key));
        var playerIds = Data.Players.Select(p => p.Id).ToHashSet();
        foreach (var id in savedPlayers.Keys.Where(id => !playerIds.Contains(id)))
            Execute("DELETE FROM players WHERE id=@id", ("@id", id));
        var extras = JsonSerializer.Serialize(Data.Extras);
        if (extras != savedExtras) Execute("INSERT INTO progression(id,payload) VALUES(1,@payload) ON DUPLICATE KEY UPDATE payload=VALUES(payload)", ("@payload",extras));
        transaction.Commit(); Remember();
        } catch {
            Data = new LeagueData { Players = savedPlayers.Values.Select(v => JsonSerializer.Deserialize<Player>(v)!).ToList(), Tickets = savedTickets.Values.Select(v => JsonSerializer.Deserialize<Ticket>(v)!).ToList(), Transactions = new(savedTransactions), Extras = JsonSerializer.Deserialize<ExtraState>(savedExtras)! };
            throw;
        }
    }
    public RankRow[] Ranks() => Data.Players.Where(p => p.Score > 0)
        .OrderByDescending(p => p.Score).ThenBy(p => p.RecordAt).ThenBy(p => p.Id, StringComparer.Ordinal)
        .Select((p, i) => new RankRow(p.Id, p.Name, p.Score, i + 1)).ToArray();
}
