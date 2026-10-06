import '../app/lib/game/engine.dart';
import 'route_driver.dart';

void check(bool ok, String reason) {
  if (!ok) throw StateError(reason);
}

void crossing(
  GameEngine g,
  ItemKind kind, {
  int lane = 3,
  ObstacleMotion motion = ObstacleMotion.still,
}) {
  g.items.clear();
  g.items.add(
    TrackItem(g.distance + .01, lane, kind, 30, motion: motion, endLane: lane),
  );
  g.update(1 / 60);
}

void main() {
  for (final kind in powerNames.keys) {
    final g = GameEngine(const LevelSpec(1), seed: 1);
    crossing(g, kind);
    check(g.powered(kind), 'Power pickup $kind');
    g.powerEnds[kind] = g.ticks + 1;
    g.update(1 / 60);
    check(!g.powered(kind), 'Power expiry $kind');
  }
  final shield = GameEngine(const LevelSpec(1), seed: 1);
  crossing(shield, ItemKind.shield);
  crossing(shield, ItemKind.barrier);
  check(shield.hits == 0 && shield.shields == 0, 'Shield absorbs once');
  shield.invulnerable = 0;
  crossing(shield, ItemKind.barrier);
  check(shield.hits == 1, 'Consumed shield cannot absorb twice');
  final ghost = GameEngine(const LevelSpec(1), seed: 1);
  crossing(ghost, ItemKind.ghost);
  crossing(ghost, ItemKind.barrier);
  check(ghost.hits == 0, 'Ghost passes barriers');
  ghost.x = 1.7;
  ghost.steer(1.7);
  ghost.update(1 / 60);
  check(ghost.hits == 1, 'Ghost cannot bypass edge fall');
  final magnet = GameEngine(const LevelSpec(1), seed: 1);
  crossing(magnet, ItemKind.magnet);
  crossing(magnet, ItemKind.crystal, lane: 1);
  check(magnet.crystals == 1, 'Magnet attracts distant crystal');
  final doubled = GameEngine(const LevelSpec(1), seed: 1);
  crossing(doubled, ItemKind.doubleScore);
  crossing(doubled, ItemKind.doubleCrystal);
  crossing(doubled, ItemKind.crystal);
  check(doubled.score == 220 && doubled.crystals == 2, 'Double powers combine');
  final shrunk = GameEngine(const LevelSpec(1), seed: 1);
  crossing(shrunk, ItemKind.shrink);
  shrunk.x = .39;
  shrunk.steer(.39);
  crossing(shrunk, ItemKind.barrier);
  check(
    shrunk.hits == 0 && shrunk.nearMisses == 1 && shrunk.score == 50,
    'Shrink and near miss',
  );
  final combo = GameEngine(const LevelSpec(1), seed: 1);
  for (int i = 0; i < 20; i++) crossing(combo, ItemKind.crystal);
  check(combo.multiplier == 5, 'Combo reaches x5');
  crossing(combo, ItemKind.crystal, lane: 0);
  check(
    combo.combo == 20 && combo.multiplier == 5,
    'Optional crystals do not break combo',
  );
  crossing(combo, ItemKind.barrier);
  check(combo.combo == 0, 'Damage breaks combo');
  final turbo = GameEngine(const LevelSpec(1), seed: 1);
  final speed = turbo.speed;
  crossing(turbo, ItemKind.turbo);
  check(turbo.speed > speed * 1.24, 'Turbo increases speed');
  final milestones = <int>{};
  final kinds = <ItemKind>{};
  final motions = <ObstacleMotion>{};
  final worlds = <String>{};
  final g = GameEngine(const LevelSpec(1), seed: 99);
  for (int t = 0; t < 60 * 420; t++) {
    for (final item in g.items) {
      kinds.add(item.kind);
      motions.add(item.motion);
    }
    worlds.add(g.world);
    g.steer(primaryTarget(g));
    g.update(1 / 60);
    if (g.wow > 0) milestones.add(g.wow);
    check(!g.done && g.hits == 0, 'Safe corridor remains reachable');
  }
  check(kinds.containsAll(powerNames.keys), 'All seven powers spawn');
  check(
    motions.containsAll({
          ObstacleMotion.still,
          ObstacleMotion.portalSide,
          ObstacleMotion.portalVertical,
        }) &&
        motions.length == 3,
    'Small blocks stay static; only full-width gates move',
  );
  check(
    milestones.containsAll([5000, 10000, 15000, 20000]),
    'All four special scenes trigger',
  );
  check(worlds.length == 14, 'All 14 worlds reachable');
  print(
    'PASS V9: seven pickups/expiry, shield, ghost, magnet, x2 stacking, shrink, near-miss, x5 combo, turbo, 14 worlds, static small blocks, full-width gates and four milestones',
  );
}
