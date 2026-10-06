import '../app/lib/game/engine.dart';
import 'route_driver.dart';

void check(bool value, String message) {
  if (!value) throw StateError(message);
}

void main() {
  final seen = <ObstacleMotion>{};
  for (int seed = 1; seed <= 40; seed++) {
    final g = GameEngine(const LevelSpec(1), seed: seed);
    double lastGate = 0;
    bool lastWasBoss = false;
    for (int t = 0; t < 60 * 420; t++) {
      for (final i in g.items.where((i) => i.moving)) {
        check(i.portal, 'Small moving obstacles must not spawn');
        check(
          GameEngine.lanesAt(i.z - 16).length == 6 &&
              GameEngine.lanesAt(i.z + 16).length == 6,
          'Full width platform required',
        );
        check(i.openingX(i.z - 12).abs() <= 1.2 + 1e-9, 'Opening outside road');
        check(
          !i.blocks(
            i.motion == ObstacleMotion.portalSide
                ? i.openingX(g.ticks.toDouble())
                : i.x,
            g.ticks.toDouble(),
          ),
          'Reachable gateway required',
        );
        seen.add(i.motion);
      }
      final crossing = g.items
          .where(
            (i) =>
                i.portal &&
                !i.resolved &&
                i.z > g.distance &&
                i.z <= g.distance + g.speed / 60,
          )
          .toList();
      g.steer(primaryTarget(g));
      g.update(1 / 60);
      for (final gate in crossing) {
        check(
          gate.boss || lastWasBoss || gate.z - lastGate > 300,
          'Only boss sequence can use rapid gates',
        );
        lastGate = gate.z;
        lastWasBoss = gate.boss;
      }
      check(g.hits == 0, 'Route failed seed=$seed tick=$t distance=${g.distance} x=${g.x} fall=${g.lastWasFall}');
    }
  }
  check(
    seen.containsAll({
      ObstacleMotion.portalSide,
      ObstacleMotion.portalVertical,
    }),
    'Both portals required',
  );
  final side = TrackItem(
    .01,
    0,
    ItemKind.barrier,
    1,
    motion: ObstacleMotion.portalSide,
  );
  final g = GameEngine(const LevelSpec(1), seed: 1);
  g.items.clear();
  g.items.add(side);
  g.update(1 / 60);
  check(g.hits == 1, 'Full-width panel must damage outside opening');
  final vertical = TrackItem(
    100,
    3,
    ItemKind.barrier,
    1,
    motion: ObstacleMotion.portalVertical,
  );
  check(vertical.blocks(-.24, 103), 'Raised panel is a real timed hazard');
  check(!vertical.blocks(.24, 103), 'Escape slot remains open');
  check(!vertical.blocks(-.24, 223), 'Broad opening changes with clock');
  check(side.openingX(-8) != side.openingX(.01), 'Side opening must move');
  print(
    'PASS V9.1: 40 seven-minute safe paths, full-width gates, only static small obstacles, side collision and vertical aperture',
  );
}
