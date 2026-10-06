import '../app/lib/data/challenges.dart';
import '../app/lib/game/engine.dart';

void check(bool condition) {
  if (!condition) throw StateError('Challenge assertion');
}

RunResult run(
  String id,
  int crystals, {
  int combo = 10,
  double distance = 600,
}) => RunResult(
  id: id,
  score: 0,
  crystals: crystals,
  hits: 0,
  ticks: 0,
  distance: distance,
  continued: false,
  maxCombo: combo,
  shieldUses: 2,
  cleanMeters: 600,
);
void main() {
  final d = DailyChallenges();
  d.record(run('a', 30), '2026-10-03');
  d.record(run('a', 30), '2026-10-03');
  check(d.crystals == 30);
  d.record(run('a', 35), '2026-10-03');
  check(d.crystals == 35);
  for (final task in d.tasks) {
    if (task.completed) {
      check(d.claim(task.id) == task.reward);
      check(d.claim(task.id) == 0);
    }
  }
  final copy = DailyChallenges()..load(d.toJson());
  check(copy.crystals == 35 && copy.claimed.length == d.claimed.length);
  copy.ensureDay('2026-10-04');
  check(copy.crystals == 0 && copy.claimed.isEmpty && copy.counted.isEmpty);
  copy.record(run('b', 3, combo: 2, distance: 20), '2026-10-04');
  check(copy.claim('crystals') == 0);
  copy.load({'crystals': double.nan, 'distance': double.infinity});
  check(copy.crystals == 0 && copy.distance == 0);
  print(
    'PASS daily progress, revive deltas, duplicate claims, persistence, reset and malformed input',
  );
}
