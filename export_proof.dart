import 'dart:convert';
import '../app/lib/game/engine.dart';

void main(List<String> args) {
  final seed = int.parse(args.first);
  final g = GameEngine(const LevelSpec(1), seed: seed, runId: 'proof');
  while (!g.done && g.ticks < 216000) {
    if (g.ticks < 3600) {
      g.steer(
        g.items
            .firstWhere((i) => i.kind == ItemKind.crystal && i.z > g.distance)
            .x,
      );
    } else {
      g.steer(1.7);
    }
    g.update(1 / 60);
  }
  print(
    jsonEncode({
      'seed': seed,
      'score': g.score,
      'ticks': g.result!.ticks,
      'distance': g.distance,
      'events': g.inputs.map((e) => e.toJson()).toList(),
    }),
  );
}
