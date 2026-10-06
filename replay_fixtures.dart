import 'dart:convert';
import '../app/lib/game/engine.dart';
import 'route_driver.dart';

void main() {
  final fixtures = <Object>[];
  for (final seed in [1, 6, 67, 81, 99]) {
    for (final mode in ['idle', 'zigzag', 'powers']) {
      final g = GameEngine(
        const LevelSpec(1),
        seed: seed,
        runId: '$seed-$mode',
      );
      while (!g.done && g.ticks < 40000) {
        if (mode == 'zigzag') g.steer(g.ticks % 420 < 210 ? -.7 : .7);
        if (mode == 'powers') {
          g.steer(g.ticks < 24000 ? primaryTarget(g) : 1.7);
        }
        if (g.ticks >= 24000) g.steer(1.7);
        g.update(1 / 60);
      }
      if (!g.done) throw StateError('Fixture did not end');
      final r = g.result!;
      fixtures.add({
        'seed': seed,
        'mode': mode,
        'ticks': r.ticks,
        'score': r.score,
        'crystals': r.crystals,
        'distance': r.distance,
        'events': g.inputs.map((e) => e.toJson()).toList(),
      });
    }
  }
  for (final seed in [1, 99]) {
    for (final mode in [RunMode.daily, RunMode.crystals, RunMode.survival]) {
      final g = GameEngine(
        const LevelSpec(1),
        seed: seed,
        runId: '$seed-${mode.name}',
        mode: mode,
      );
      while (!g.done && g.ticks < 40000) {
        g.steer(g.ticks < 24000 ? primaryTarget(g) : 1.7);
        g.update(1 / 60);
      }
      if (!g.done) throw StateError('Mode did not end');
      final r = g.result!;
      fixtures.add({
        'seed': seed,
        'mode': mode.name,
        'runMode': mode.name,
        'ticks': r.ticks,
        'score': r.score,
        'crystals': r.crystals,
        'distance': r.distance,
        'events': g.inputs.map((e) => e.toJson()).toList(),
      });
    }
  }
  print(jsonEncode(fixtures));
}
