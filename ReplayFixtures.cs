using System;
using System.IO;
using System.Linq;
using System.Text.Json;

public static class ReplayFixtures {
    public static void Verify(string path, int version = 14) {
        using var data = JsonDocument.Parse(File.ReadAllText(path));
        int count = 0;
        foreach (var item in data.RootElement.EnumerateArray()) {
            var events = item.GetProperty("events").EnumerateArray()
                .Select(e => new SteeringEvent(e.GetProperty("tick").GetInt32(), e.GetProperty("value").GetInt32())).ToArray();
            var result = ReplayEngine.Verify(item.GetProperty("seed").GetInt32(), new(item.GetProperty("ticks").GetInt32(), events), version, item.TryGetProperty("runMode", out var m) ? m.GetString()! : "endless");
            if (result is null || result.Score != item.GetProperty("score").GetInt32() ||
                result.Crystals != item.GetProperty("crystals").GetInt32() ||
                Math.Abs(result.Distance - item.GetProperty("distance").GetDouble()) > .00001) {
                throw new InvalidOperationException($"Dart/C# replay mismatch: seed {item.GetProperty("seed")}, mode {item.GetProperty("mode")}");
            }
            count++;
        }
        Console.WriteLine($"PASS {count} Dart/C# replay fixtures");
    }
}
