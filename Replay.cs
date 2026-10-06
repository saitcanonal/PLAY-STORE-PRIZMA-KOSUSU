public record SteeringEvent(int Tick, int Value);
public record FinishInput(int Ticks, SteeringEvent[] Events);
public record ReplayResult(int Score, int Crystals, int Ticks, double Distance);
public sealed class ReplayEngine {
    private sealed class Item(double z, int lane, int kind, int row = 0, int motion = 0, int endLane = 0, bool version9 = false, double center = 2, bool version13 = false) {
        public double Z = z;
        public int Lane = lane, Kind = kind, Row = row, Motion = motion, EndLane = endLane;
        public double PhaseAt(int tick) {
            var phase = (tick + Row * 17) % 240;
            return phase <= 120 ? phase / 120.0 : (240 - phase) / 120.0;
        }
        public double XAt(int tick) => (Lane - center) * .48 +
            ((Motion == 1 || Motion == 6) ? (EndLane - Lane) * .48 * PhaseAt(tick) + (version9 ? (PhaseAt(tick) - .5) * .16 : 0) : 0);
        public bool ActiveAt(int tick) => Motion is 2 or 3 or 7 or 8 or 9 ? PhaseAt(tick) >= .55 : true;
        public bool Portal => Motion is 4 or 5;
        public double PortalPhase(double distance) {
            if(version13) { var phase=((int)distance+Row*17)%240;return phase<=120?phase/120.0:(240-phase)/120.0; }
            var t = (Math.Abs(Z - distance) % 32) / 16;
            return t <= 1 ? t : 2 - t;
        }
        public bool Blocks(double position, double distance) => Motion == 4
            ? Math.Abs(position - ((Lane - center) * .48 + (Lane >= Math.Ceiling(center) ? -.48 : .48) * PortalPhase(distance))) > .30
            : .8 * PortalPhase(distance) > .20 && (!version13 || Math.Abs(position-(Lane-center)*.48)>.24);
        public bool Resolved;
    }
    private readonly int seed;
    private long random;
    private int row, safe = 2, combo, crystals, hits, crystalScore, ticks;
    private double nextZ = 14, nextPortal = 480, nextGateway = 480, distance, x, invulnerable, recovery;
    private readonly List<Item> items = [];
    public bool Done { get; private set; }
    private int gatewayCount, portalCount, shields, nearRow = -1, nextWow = 5000, wow, wowEnds;
    private readonly Dictionary<int, int> powers = [];
    private bool Powered(int kind) => powers.GetValueOrDefault(kind) > ticks;
    private readonly int version;
    private readonly string mode;
    public ReplayEngine(int seed, int version = 14, string mode = "endless") { this.seed=seed;this.version = version; this.mode = mode; random = seed; if(version>=12) { safe=3; x=.24; } Generate(); }
    private double Next() { random = (random * 1103515245 + 12345) & 0x7fffffff; return random / 2147483648.0; }
    private double Center => version >= 12 ? 2.5 : 2;
    private int LaneCount => version >= 12 ? 6 : 5;
    private int[] WideLanes => version >= 12 ? [0,1,2,3,4,5] : [0,1,2,3,4];
    private int[] LanesAt(double z) {
        if(version>=13) {
            var world=((int)(z/500)+5)%14;var local=z%500;
            if(world==8 && local>=100 && local<400) return [2,3];
            if(world==11 && local>=100 && local<400) return [0,2,3,4,5];
        }
        if (z < 450) return WideLanes;
        var cycle = (int)Math.Floor((z - 450) / 180);
        var phase = (z - 450) % 180;
        var identity = (((int)(z / 500)) + 5) % 14 % 3;
        if (version >= 11 && z >= 1000 && identity == 1) return WideLanes;
        var side = cycle % 2 == 0 ? (version >= 12 ? 2 : 1) : (version >= 12 ? 3 : 2);
        if (phase < 16) return [side, side + 1];
        if (phase < 56) return version >= 11 && z >= 1000 && identity == 2 ? [side, side + 1] : [version >= 12 ? 3 : 2];
        if (phase < 72) return [side, side + 1];
        return WideLanes;
    }
    private bool OnPlatform(double x, double z) => LanesAt(z).Any(l => Math.Abs(x - (l - Center) * .48) <= .24);
    private void GenerateV11() {
        while (nextZ < distance + 100) {
            var gap = Math.Min(15.0, 8.0 + nextZ / 500);
            var lanes = LanesAt(nextZ);
            var safeLanes = lanes.Where(l => Enumerable.Range(-24,49).All(offset => LanesAt(nextZ + offset).Contains(l))).ToArray();
            var theme = (((int)(nextZ / 500)) + 5) % 14;
            var piece = (((int)Math.Floor(nextZ / 96)) + theme) % 4;
            if (piece == 0 && row > 2) safe = Math.Clamp(safe + (row / 4 % 2 == 0 ? 1 : -1), 0, LaneCount - 1);
            else if (piece == 2 && row % 4 == 0) safe = Math.Clamp(safe + (Next() < .5 ? 1 : -1), 0, LaneCount - 1);
            if (!safeLanes.Contains(safe)) safe = safeLanes.OrderBy(l => Math.Abs(l - safe)).First();
            items.Add(new(nextZ, safe, 0, row, center: Center));
            var transition = Enumerable.Range(-24, 49).Any(offset => LanesAt(nextZ + offset).Length != lanes.Length);
            var difficulty = Math.Min(.65, .25 + nextZ / 6500) + (theme % 3 == 2 ? .06 : 0);
            var rowStart = items.Count;
            foreach (var lane in lanes)
                if (lane != safe && row > 4 && lanes.Length == LaneCount && !transition && piece != 3 && Next() < difficulty)
                    items.Add(new(nextZ, lane, 1, row, center: Center));
            if (theme % 3 == 2 && nextZ >= nextGateway && lanes.Length == LaneCount && !transition && piece != 3) {
                items.RemoveRange(rowStart, items.Count - rowStart);
                items.Add(new(nextZ, safe, 1, row, gatewayCount % 2 == 0 ? 4 : 5, center: Center));
                gatewayCount++; nextGateway = nextZ + 500;
            }
            if (theme % 3 == 1 && row > 4 && !transition) items.Add(new(nextZ + gap / 2, safe, 0, row, center: Center));
            if (row > 8 && row % 14 == 0) items.Add(new(nextZ - 3, safe, piece == 2 ? 5 : 2 + ((row / 14 - 1) % 7), row, center: Center));
            row++; nextZ += gap;
        }
    }
    private static readonly int[][] Lines=[[2, 2, 3, 3, 4, 4, 3, 3], [3, 3, 2, 2, 1, 1, 2, 2], [2, 3, 4, 4, 3, 2, 1, 1], [3, 2, 1, 1, 2, 3, 4, 4], [2, 2, 2, 3, 3, 3, 2, 2], [3, 3, 3, 2, 2, 2, 3, 3], [1, 2, 3, 4, 4, 3, 2, 1], [4, 3, 2, 1, 1, 2, 3, 4], [2, 3, 2, 3, 2, 3, 2, 3], [3, 2, 3, 2, 3, 2, 3, 2], [1, 1, 2, 2, 3, 3, 4, 4], [4, 4, 3, 3, 2, 2, 1, 1], [2, 2, 3, 4, 4, 3, 2, 2], [3, 3, 2, 1, 1, 2, 3, 3], [2, 3, 3, 2, 2, 1, 1, 2], [3, 2, 2, 3, 3, 4, 4, 3], [2, 2, 2, 2, 3, 3, 3, 3], [3, 3, 3, 3, 2, 2, 2, 2], [2, 3, 4, 3, 2, 1, 2, 3], [3, 2, 1, 2, 3, 4, 3, 2]];
    private void GenerateV13() {
        while(nextZ<distance+100) {
            var gap=Math.Min(15.0,8.0+nextZ/500);var lanes=LanesAt(nextZ);
            var safeLanes=lanes.Where(l=>Enumerable.Range(-24,49).All(d=>LanesAt(nextZ+d).Contains(l))).ToArray();
            var theme=((int)(nextZ/500)+5)%14;var piece=(row/8*7+seed%20)%20;var step=row%8;
            var desired=Lines[piece][step];
            if(theme is 5 or 13) desired=new[]{2,2,3,3,4,4,3,3}[step];
            if(theme==9) desired=new[]{2,3,4,3,2,1,2,3}[step];
            if(theme is 8 or 11) desired=3;
            var previousSafe=safe;if(row>4) safe+=Math.Clamp(desired-safe,-1,1);
            if(!safeLanes.Contains(safe)) safe=safeLanes.OrderBy(l=>Math.Abs(l-safe)).First();
            var transition=Enumerable.Range(-24,49).Any(d=>LanesAt(nextZ+d).Length!=lanes.Length);
            var lava=theme==11;
            var risk=(piece is 12 or 13 || lava) && lanes.Contains(lava?0:5) && row>12 && (!transition || lava);
            var riskLane=lava?0:5;var rest=piece is 16 or 17;
            items.Add(new(nextZ,safe,0,row,center:Center,version13:true));
            if(row>4 && !transition && lanes.Length==6 && !rest) foreach(var lane in lanes) {
                if(lane==safe || lane==previousSafe || (risk && (lane==3 || lane==riskLane))) continue;
                if((lane+step+piece)%3==0 || theme==9) items.Add(new(nextZ,lane,1,row,center:Center,version13:true));
            }
            if(risk) {
                if(!lava && step is 3 or 7) items.Add(new(nextZ,riskLane,1,row,center:Center,version13:true));
                else { for(int j=0;j<3;j++) items.Add(new(nextZ+j*gap/3,riskLane,0,row,center:Center,version13:true));
                    if(step==1) items.Add(new(nextZ-2,riskLane,5,row,center:Center,version13:true)); }
            }
            if(theme%3==2 && nextZ>=nextGateway && lanes.Length==6 && !transition && !rest && !risk) {
                items.RemoveAll(i=>i.Row==row && i.Kind==1);
                items.Add(new(nextZ,safe,1,row,gatewayCount%2==0?4:5,center:Center,version13:true));
                gatewayCount++; nextGateway=nextZ+500;
            }
            if(theme is 5 or 13 or 8 && !transition) items.Add(new(nextZ+gap/2,safe,0,row,center:Center,version13:true));
            if(row>8 && row%14==0) items.Add(new(nextZ-3,risk?riskLane:safe,2+(row/14-1)%7,row,center:Center,version13:true));
            row++; nextZ+=gap;
        }
    }
    private void Guardian() {
        var start=distance+108;
        for(int wave=0;wave<3;wave++) {
            var z=start+wave*24;var lanes=LanesAt(z);var preferred=wave==1?2:3;
            var opening=lanes.Contains(preferred)?preferred:lanes[0];
            foreach(var lane in lanes) items.Add(new(z,lane,lane==opening?0:1,100000+wave,center:Center,version13:true));
        }
        nextZ=start+80;
    }
    private void Generate() {
        if(version>=13) {GenerateV13();return;}
        if (version >= 11) { GenerateV11(); return; }
        while (nextZ < distance + 100) {
            var lanes = LanesAt(nextZ);
            var safeLanes = lanes.Where(l => LanesAt(nextZ - 8).Contains(l) && LanesAt(nextZ + 8).Contains(l)).ToArray();
            if (row > 2 && Next() < .58) safe = Math.Clamp(safe + (Next() < .5 ? 1 : -1), 0, 4);
            if (!safeLanes.Contains(safe)) safe = safeLanes.OrderBy(l => Math.Abs(l - safe)).First();
            items.Add(new(nextZ, safe, 0));
            var difficulty = Math.Min(.75, .30 + nextZ / 6500);
            var transition = LanesAt(nextZ - 12).Length != lanes.Length || LanesAt(nextZ + 12).Length != lanes.Length;
            var rowStart = items.Count;
            for (int lane = 0; lane < 5; lane++)
                if (lane != safe && lanes.Contains(lane) && row > 4 && lanes.Length == 5 && !transition && Next() < difficulty)
                    if (version >= 9) {
                        var motion = version == 9 && row >= 20 && row % 4 == 0 ? new[] { 1, 2, 3, 6, 7, 8, 9 }[(row / 4 + lane) % 7] : 0;
                        items.Add(new(nextZ, lane, 1, row, motion, lane, true));
                    } else items.Add(new(nextZ, lane, 1));
            if (version >= 6 && version < 8 && nextZ >= (version >= 7 ? 150 : 430) && row % 6 == 0 && lanes.Length == 5 &&
                !transition && LanesAt(nextZ - 36).Length == 5 && LanesAt(nextZ + 36).Length == 5) {
                var left = safe >= 2 ? 0 : 3;
                items.RemoveRange(rowStart, items.Count - rowStart);
                for (int lane = 0; lane < 5; lane++) {
                    if (lane != safe && lane != left && lane != left + 1 && (row + lane) % 3 == 0)
                        items.Add(new(nextZ, lane, 1, row));
                }
                items.Add(new(nextZ, left, 1, row, 1 + (row / 6) % 3, left + 1));
            }
            if (version >= 8 && (nextZ >= nextPortal || (version >= 9 && wow == 20000 && ticks < wowEnds && row % 6 == 0)) && lanes.Length == 5 &&
                LanesAt(nextZ - 16).Length == 5 && LanesAt(nextZ + 16).Length == 5) {
                items.RemoveRange(rowStart, items.Count - rowStart);
                items.Add(new(nextZ, safe, 1, row, portalCount % 2 == 0 ? 4 : 5));
                portalCount++; if (nextZ >= nextPortal) nextPortal += 500;
            }
            if (version >= 9 && row > 8 && row % 14 == 0) items.Add(new(nextZ - 3, safe, 2 + ((row / 14 - 1) % 7), row));
            row++; nextZ += 8;
        }
    }
    private void Damage(bool fall) {
        if (invulnerable > 0 || Done) return;
        if (version >= 9 && !fall && Powered(7)) return;
        if (version >= 9 && !fall && shields > 0 && Powered(2)) {
            shields = 0; powers.Remove(2); combo = 0; invulnerable = .4; return;
        }
        hits++; combo = 0;
        shields = 0; powers.Remove(2);
        invulnerable = 1.8; recovery = fall ? .65 : .18;
        if (hits >= (mode == "survival" ? 1 : 3)) Done = true;
    }
    public void Step(double input) {
        if (Done) return;
        const double dt = 1.0 / 60;
        var before = distance;
        var elapsed = ticks / 60.0;
        var speed = elapsed <= 56.25 ? 10.5 + elapsed * .24 : 24.0 + 2.0 * Math.Log(1 + (elapsed - 56.25) / 60);
        if (version >= 14) speed = elapsed <= 56.25 ? 10.5 + elapsed * .24 : 24.0 + (elapsed - 56.25) * .12;
        else if (version >= 11) speed = Math.Min(24, speed);
        if (version >= 9 && Powered(8)) speed *= 1.25;
        distance += speed * dt;
        ticks++;
        if (!Powered(2)) shields = 0;
        invulnerable = Math.Max(0, invulnerable - dt);
        var recovering = recovery > 0;
        if (recovering) {
            recovery = Math.Max(0, recovery - dt);
            if (recovery == 0) x = (LanesAt(distance).OrderBy(l => Math.Abs(x - (l - Center) * .48)).First() - Center) * .48;
        } else {
            x += (input - x) * (1 - Math.Exp(-17 * dt));
            if (!OnPlatform(x, distance)) Damage(true);
        }
        foreach (var item in items) {
            if (Done) break;
            if (item.Resolved || item.Z > distance || item.Z < before) continue;
            item.Resolved = true;
            if (recovering || recovery > 0 || !OnPlatform(x, distance)) continue;
            var separation = Math.Abs(x - item.XAt(ticks));
            var threshold = version >= 9 && Powered(6) ? .10 : .18;
            var touching = separation < threshold;
            if (item.Kind == 0) {
                if (touching || (version >= 9 && Powered(3) && separation < 1.25)) {
                    crystals += version >= 9 && Powered(5) ? 2 : 1;
                    combo++;
                    crystalScore += (100 + Math.Min(combo, 10) * 10) * (version >= 9 ? (1 + Math.Min(4, combo / 5)) * (Powered(4) ? 2 : 1) : 1);
                }
                else if(version<13) combo=0;
            } else if (item.Kind == 1 && (item.Portal ? item.Blocks(x,version>=13?ticks:distance) : item.ActiveAt(ticks) && touching)) Damage(version >= 9 && item.Motion == 9);
            else if (version >= 9 && item.Kind == 1 && !item.Portal && item.ActiveAt(ticks) && separation >= threshold && separation < .32 && invulnerable == 0 && !Powered(7) && nearRow != item.Row) {
                nearRow = item.Row; crystalScore += 50;
            } else if (version >= 9 && item.Kind >= 2 && item.Kind <= 8 && touching) {
                powers[item.Kind] = ticks + (item.Kind == 8 ? 300 : 480);
                if (item.Kind == 2) shields = 1;
            }
        }
        if (version >= 9 && crystalScore >= nextWow && nextWow <= 20000) {
            wow = nextWow; nextWow += 5000; wowEnds = ticks + 480;
            if(version>=13 && wow==20000) Guardian();
        }
        items.RemoveAll(i => i.Z < distance - 8);
        Generate();
        if (mode == "daily" && ticks >= 5400 || mode == "crystals" && ticks >= 3600 || version >= 11 && ticks >= 216000) Done = true;
    }
    public ReplayResult Result => new(crystalScore, crystals, ticks, distance);
    public static ReplayResult? Verify(int seed, FinishInput input, int version = 14, string mode = "endless") {
        if (input.Ticks < 1 || input.Ticks > 216000 || input.Events is null || input.Events.Length > input.Ticks) return null;
        int last = -1;
        foreach (var e in input.Events) {
            if (e is null) return null;
            if (e.Tick <= last || e.Tick < 0 || e.Tick >= input.Ticks || e.Value < -(version >= 12 ? 1700 : 1400) || e.Value > (version >= 12 ? 1700 : 1400)) return null;
            last = e.Tick;
        }
        var game = new ReplayEngine(seed, version, mode);
        int cursor = 0, value = 0;
        for (int t = 0; t < input.Ticks; t++) {
            if (cursor < input.Events.Length && input.Events[cursor].Tick == t) value = input.Events[cursor++].Value;
            game.Step(value / 1000.0);
            if (game.Done && t + 1 != input.Ticks) return null;
        }
        return game.Done ? game.Result : null;
    }
}
