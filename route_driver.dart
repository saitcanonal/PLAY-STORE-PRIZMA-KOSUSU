import '../app/lib/game/engine.dart';

// The player follows the primary reward line except when a timed gate requires
// steering to its visible opening. Optional edge rewards are deliberately ignored.
double primaryTarget(GameEngine g) {
  final next = g.items.firstWhere(
    (i) => i.kind == ItemKind.crystal && !i.rewardRoute && i.z > g.distance,
  );
  final portals = g.items.where(
    (i) => i.portal && i.z > g.distance && i.z - g.distance < 14,
  );
  if (portals.isNotEmpty && !g.items.any((i) =>
      i.kind == ItemKind.barrier && !i.portal && !i.resolved &&
      i.z > g.distance && i.z < portals.first.z)) {
    final gate = portals.first;
    return gate.motion == ObstacleMotion.portalSide
        ? gate.openingX(g.ticks + (gate.z - g.distance) / g.speed * 60)
        : gate.x;
  }
  return next.x;
}
