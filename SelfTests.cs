public static class SelfTests {
    public static void Run() {
        static void Assert(bool value, string message) { if (!value) throw new Exception(message); }
        for (int seed = 1; seed <= 40; seed++) {
            var a = new ReplayEngine(seed);
            while (!a.Done && a.Result.Ticks < 216000) a.Step(0);
            Assert(a.Done, "Stationary run must eventually end");
            var valid = ReplayEngine.Verify(seed, new(a.Result.Ticks, []));
            Assert(valid is not null && valid.Score == a.Result.Score, "Replay must reproduce score");
            Assert(ReplayEngine.Verify(seed, new(a.Result.Ticks + 1, [])) is null, "Extra tick after death rejected");
            Assert(ReplayEngine.Verify(seed, new(a.Result.Ticks, [new(0, 1701)])) is null, "Out of bounds steering rejected");
            Assert(ReplayEngine.Verify(seed, new(a.Result.Ticks, [new(2, 10), new(1, 20)])) is null, "Unsorted steering rejected");
        }
        var historical = new ReplayEngine(1, 8);
        while (!historical.Done && historical.Result.Ticks < 216000) historical.Step(0);
        Assert(ReplayEngine.Verify(1, new(historical.Result.Ticks, []), 8)?.Score == historical.Result.Score, "V8 tickets remain verifiable");
        var historical9 = new ReplayEngine(1, 9);
        while (!historical9.Done && historical9.Result.Ticks < 216000) historical9.Step(0);
        Assert(ReplayEngine.Verify(1, new(historical9.Result.Ticks, []), 9)?.Score == historical9.Result.Score, "V9 moving obstacle tickets remain verifiable");
        var falling = new ReplayEngine(1);
        for (int t = 0; t < 60; t++) falling.Step(1.7);
        Assert(falling.Result.Distance > 0 && falling.Result.Score == 0, "Distance without pickup awards no score");
        while (!falling.Done && falling.Result.Ticks < 3000) falling.Step(1.7);
        Assert(falling.Done, "Three edge falls must terminate the run");
        Assert(ReplayEngine.Verify(1, new(falling.Result.Ticks, [new(0, 1700)]))?.Score == falling.Result.Score, "Fall replay agrees");
        Assert(ReplayEngine.Verify(1, new(0, [])) is null, "Zero duration rejected");
        Console.WriteLine("Replay and invalid input tests passed. Run scripts/test_api.py against a dedicated MySQL test database for persistence/security.");
    }
}
