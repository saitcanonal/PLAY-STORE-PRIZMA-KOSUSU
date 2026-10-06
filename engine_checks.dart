// Runs the actual production Dart engine without the Flutter UI/runtime.
import '../app/lib/game/engine.dart';
import 'route_driver.dart';

void check(bool value, String label) {
  if (!value) throw StateError(label);
}

void main() {
  for (int seed = 1; seed <= 40; seed++) {
    final game = GameEngine(const LevelSpec(1), seed: seed);
    while (!game.done && game.ticks < 216000) {
      game.update(1 / 60);
    }
    check(game.done && game.hits == 3, 'Seed $seed did not finish');
  }
  for (final seed in [1, 6, 67, 81, 99]) {
    final game = GameEngine(const LevelSpec(1), seed: seed);
    int maxItems = 0;
    for (int tick = 0; tick < 60 * 60 * 15; tick++) {
      game.steer(primaryTarget(game));
      game.update(1 / 60);
      check(
        !game.done && game.hits == 0,
        'Unsafe bridge seed $seed tick $tick',
      );
      for (final item in game.items.where(
        (i) => i.kind == ItemKind.barrier && !i.boss,
      )) {
        check(
          GameEngine.lanesAt(item.z).length == 6 &&
              GameEngine.lanesAt(item.z - 12).length == 6 &&
              GameEngine.lanesAt(item.z + 12).length == 6,
          'Blocked narrow bridge',
        );
      }
      if (game.items.length > maxItems) maxItems = game.items.length;
    }
    check(game.speed > 60 && maxItems < 90, 'Continuous speed or bounded object count');
    print(
      'PASS Dart seed $seed: ${game.distance.toStringAsFixed(1)}m, ${game.hits} hits, $maxItems objects',
    );
  }
  final frames = <RunResult>[];
  for (final fps in [20, 60, 120]) {
    final game = GameEngine(const LevelSpec(1), seed: 67);
    while (!game.done && game.ticks < 216000) {
      game.update(1 / fps);
    }
    frames.add(game.result!);
  }
  check(
    frames.map((r) => r.ticks).toSet().length == 1 &&
        frames.map((r) => r.score).toSet().length == 1,
    'Frame rate changes simulation',
  );
  print(
    'PASS actual Dart engine: 40 seeds, 5 long safe paths, clear bridges, 20/60/120fps',
  );
}
