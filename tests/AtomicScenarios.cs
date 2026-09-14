using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

public static class AtomicScenarios
{
    private static GameConfig config = null!;
    public static void Run(string path)
    {
        config = GameConfig.Load(path);
        Check("path.ground_traverses_unbuildable", PathGroundTraversesUnbuildable);
        Check("build.unbuildable_blocks_place", BuildUnbuildableBlocksPlace);
        Check("path.air_ignores_ground_clutter", PathAirIgnoresGroundClutter);
        Check("combat.tank_cannot_hit_chinook", CombatTankCannotHitChinook);
        Check("combat.rocket_missile_kills_chinook", CombatRocketMissileKillsChinook);
        Check("combat.missile_exists_before_impact", CombatMissileExistsBeforeImpact);
        Check("combat.no_duplicate_missile_impact", CombatNoDuplicateMissileImpact);
        Check("path.distinguish_wait_vs_stuck", PathDistinguishWaitVsStuck);
    }
    private static void Check(string label, Action test) { test(); Console.WriteLine("PASS atomic: " + label); }
    private static void Need(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static Match New(bool clutter = false)
    {
        var c = config with { Map = config.Map with { Objects = Array.Empty<MapObjectConfig>(), Terrain = clutter ? config.Map.Terrain : Array.Empty<TerrainRect>() } };
        var s = c.CreateSetup() with { Fog = false, Slots = c.DefaultSlots.Select(p => p.Index == 1 ? p with { Occupant = Occupant.Player } : p).ToArray() };
        return (Match)new MatchFactory().Create(c, s);
    }
    private static void Send(Match m, OrderKind kind, Match.Body actor, int target = 0, string product = "", WorldPoint pos = default)
    {
        var r = m.Submit(new MatchOrder(actor.Owner, kind, new[] { actor.Id }, target, pos, product));
        Need(r.Accepted, kind + " rejected: " + r.Reason);
    }
    private static void Steps(Match m, int count) { for (int i = 0; i < count; i++) m.Step(); }
    private static void Until(Match m, Func<bool> done, int limit, string message) { for (int i = 0; i < limit && !done(); i++) m.Step(); Need(done(), message); }
    private static long Dist2(WorldPoint a, WorldPoint b) => (long)(a.X - b.X) * (a.X - b.X) + (long)(a.Z - b.Z) * (a.Z - b.Z);
    private static bool OnUnbuildable(MapConfig map, WorldPoint point)
    {
        int x = point.X / map.CellSize, z = point.Z / map.CellSize;
        return map.InMap(x, z) && map.TerrainAt(x, z) == "ter.unbuildable";
    }
    private static bool OracleGroundLegal(Match m, WorldPoint p, int radius, int ignoreId)
    {
        var map = m.C.Map;
        if (!map.InMap(new WorldPoint(p.X - radius, p.Z - radius)) || !map.InMap(new WorldPoint(p.X + radius, p.Z + radius))) return false;
        int cell = map.CellSize;
        for (int z = (p.Z - radius) / cell; z <= (p.Z + radius) / cell; z++)
            for (int x = (p.X - radius) / cell; x <= (p.X + radius) / cell; x++)
            {
                if (map.InMap(x, z) && map.GroundWalkable(map.TerrainAt(x, z))) continue;
                int nearX = Math.Clamp(p.X, x * cell, (x + 1) * cell), nearZ = Math.Clamp(p.Z, z * cell, (z + 1) * cell);
                if (Dist2(p, new WorldPoint(nearX, nearZ)) <= (long)radius * radius) return false;
            }
        foreach (var b in m.Bodies.Values)
        {
            if (b.Id == ignoreId || b.ContainerId != 0 || m.Role(b).IsFlying || !m.Role(b).IsBuilding && b.RoleId != "map.dock") continue;
            long r = radius + m.Role(b).Radius;
            if (Dist2(p, b.Pos) <= r * r) return false;
        }
        return true;
    }

    private static void PathGroundTraversesUnbuildable()
    {
        var m = New(clutter: true);
        var ground = m.Spawn("build.dozer", 0, new(4200, 1400));
        var dest = new WorldPoint(6100, 1400);
        Need(OnUnbuildable(m.C.Map, new(5100, 1400)), "fixture line must cross ter.unbuildable");
        Need(m.C.Map.GroundWalkable("ter.unbuildable") && !m.C.Map.CanBuildOn("ter.unbuildable"), "catalog walk/build split");
        Send(m, OrderKind.Move, ground, pos: dest);
        bool traversed = false;
        var prev = ground.Pos;
        int speed = config.Role("build.dozer").SpeedPerTick, radius = config.Role("build.dozer").Radius;
        for (int i = 0; i < 250; i++)
        {
            m.Step();
            Need(OracleGroundLegal(m, ground.Pos, radius, ground.Id), "ground left catalog-walkable cells or entered a building disk");
            Need(Dist2(prev, ground.Pos) <= (long)(speed + 1) * (speed + 1), "ground exceeded speed");
            if (OnUnbuildable(m.C.Map, ground.Pos)) traversed = true;
            prev = ground.Pos;
            if (ground.Actions.Count == 0 && i > 0) break;
        }
        Need(traversed, "ground must occupy ter.unbuildable (not treated as a wall)");
        Need(Dist2(ground.Pos, dest) <= (long)radius * radius, "ground must arrive across unbuildable");
    }

    private static void BuildUnbuildableBlocksPlace()
    {
        var m = New(clutter: true);
        var dozer = m.Bodies.Values.Single(b => b.Owner == 0 && b.RoleId == "build.dozer");
        var pad = new WorldPoint(5100, 1400);
        Need(OnUnbuildable(m.C.Map, pad), "place point must be ter.unbuildable");
        Need(!m.CanPlace(0, dozer.Id, "power.fusion", pad).Allowed, "CanPlace must deny unbuildable");
        Need(!m.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: pad, ProductId: "power.fusion")).Accepted, "Build must reject unbuildable");
    }

    private static void PathAirIgnoresGroundClutter()
    {
        var m = New();
        var wall = m.Spawn("power.fusion", 0, new(5100, 2000));
        var air = m.Spawn("eco.chinook", 0, new(4200, 2000)); air.AutoGather = false;
        var ground = m.Spawn("build.dozer", 0, new(4200, 2000));
        var dest = new WorldPoint(6100, 2000);
        Send(m, OrderKind.Move, air, pos: dest); Send(m, OrderKind.Move, ground, pos: dest);
        int airSpeed = config.Role("eco.chinook").SpeedPerTick;
        int deadline = DistRoot(air.Pos, dest) / Math.Max(1, airSpeed) + 8;
        var prev = air.Pos;
        for (int i = 0; i < deadline; i++)
        {
            m.Step();
            Need(Dist2(prev, air.Pos) <= (long)(airSpeed + 1) * (airSpeed + 1), "air exceeded speed");
            prev = air.Pos;
        }
        Need(Dist2(air.Pos, dest) <= (long)config.Role("eco.chinook").Radius * config.Role("eco.chinook").Radius, "air must fly over the building");
        int wallR = config.Role("power.fusion").Radius + config.Role("build.dozer").Radius;
        Need(Dist2(ground.Pos, wall.Pos) > (long)wallR * wallR, "ground must not occupy the building disk");
        Need(Dist2(ground.Pos, dest) > (long)config.Role("build.dozer").Radius * config.Role("build.dozer").Radius, "ground must still be routing around while air has arrived");
    }

    private static void CombatTankCannotHitChinook()
    {
        var m = New();
        var tank = m.Spawn("armor.basic", 0, new(6000, 1800));
        var air = m.Spawn("eco.chinook", 1, new(7000, 1800)); air.AutoGather = false;
        m.Step();
        int hp = air.Hp;
        Need(!m.Submit(new MatchOrder(0, OrderKind.Attack, new[] { tank.Id }, air.Id)).Accepted, "cannon vs air is none");
        Need(!m.Submit(new MatchOrder(0, OrderKind.ForceAttack, new[] { tank.Id }, air.Id)).Accepted, "force-attack cannot bypass the air matrix");
        Steps(m, 80);
        Need(air.Hp == hp && m.Bodies.ContainsKey(air.Id), "tank must not damage chinook");
        Need(!m.Snapshot(0).CombatTraces.Any(t => t.SourceId == tank.Id), "tank must not launch a shot at air");
        Need(m.Snapshot(0).Projectiles.Length == 0, "tank must not spawn a projectile at air");
    }

    private static void CombatRocketMissileKillsChinook()
    {
        var m = New();
        var rocket = m.Spawn("inf.rocket", 0, new(6000, 2200));
        var air = m.Spawn("eco.chinook", 1, new(7000, 1800)); air.AutoGather = false;
        Send(m, OrderKind.Attack, rocket, air.Id);
        bool sawMissile = false, sawLaunch = false, sawImpact = false;
        Until(m, () =>
        {
            var snap = m.Snapshot(0);
            if (snap.Projectiles.Any(p => p.SourceId == rocket.Id)) sawMissile = true;
            if (snap.CombatTraces.Any(t => t.SourceId == rocket.Id && t.DeliveryId == "del.missile" && t.Phase == CombatTracePhase.Launch)) sawLaunch = true;
            if (snap.CombatTraces.Any(t => t.SourceId == rocket.Id && t.Phase == CombatTracePhase.Impact && t.AppliedDamage > 0)) sawImpact = true;
            return !m.Bodies.ContainsKey(air.Id);
        }, 800, "rocket missile must kill chinook");
        Need(sawMissile && sawLaunch && sawImpact, "kill must be a del.missile with launch, in-world projectile, and damaging impact");
    }

    private static void CombatMissileExistsBeforeImpact()
    {
        var m = New();
        var rocket = m.Spawn("inf.rocket", 0, new(6000, 2200));
        var air = m.Spawn("eco.chinook", 1, new(7000, 1800)); air.AutoGather = false;
        int hp = air.Hp;
        Send(m, OrderKind.Attack, rocket, air.Id);
        m.Step();
        var snap = m.Snapshot(0);
        Need(snap.Projectiles.Length > 0 && air.Hp == hp, "missile must exist as a projectile before HP drops");
        var shot = snap.Projectiles.First(p => p.SourceId == rocket.Id);
        Need(snap.CombatTraces.Any(t => t.Id == shot.Id && t.Phase == CombatTracePhase.Launch && t.DeliveryId == "del.missile"), "launch trace shares the projectile id");
        Need(!snap.CombatTraces.Any(t => t.Id == shot.Id && t.Phase == CombatTracePhase.Impact), "impact must not precede the in-world projectile");
    }

    private static void CombatNoDuplicateMissileImpact()
    {
        var m = New();
        var rocket = m.Spawn("inf.rocket", 0, new(6000, 2200));
        var air = m.Spawn("eco.chinook", 1, new(7000, 1800)); air.AutoGather = false;
        Send(m, OrderKind.Attack, rocket, air.Id);
        int firstId = -1;
        var impacts = new Dictionary<int, int>();
        var cancels = new Dictionary<int, int>();
        int applied = 0;
        for (int i = 0; i < 400; i++)
        {
            m.Step();
            var snap = m.Snapshot(0);
            foreach (var t in snap.CombatTraces.Where(t => t.DeliveryId == "del.missile"))
            {
                if (t.Phase == CombatTracePhase.Launch && firstId < 0 && t.SourceId == rocket.Id) firstId = t.Id;
                if (t.Phase == CombatTracePhase.Impact) { impacts[t.Id] = impacts.GetValueOrDefault(t.Id) + 1; if (t.Id == firstId) applied = t.AppliedDamage; }
                if (t.Phase == CombatTracePhase.Cancel) cancels[t.Id] = cancels.GetValueOrDefault(t.Id) + 1;
            }
            if (firstId >= 0 && (impacts.GetValueOrDefault(firstId) > 0 || cancels.GetValueOrDefault(firstId) > 0) && !snap.Projectiles.Any(p => p.Id == firstId))
            {
                Steps(m, 12);
                foreach (var t in m.Snapshot(0).CombatTraces.Where(t => t.DeliveryId == "del.missile"))
                {
                    if (t.Phase == CombatTracePhase.Impact) impacts[t.Id] = impacts.GetValueOrDefault(t.Id) + 1;
                    if (t.Phase == CombatTracePhase.Cancel) cancels[t.Id] = cancels.GetValueOrDefault(t.Id) + 1;
                }
                break;
            }
        }
        Need(firstId >= 0, "missile launch id required");
        Need(impacts.Values.All(v => v <= 1), "no shot id may Impact more than once");
        Need(impacts.GetValueOrDefault(firstId) == 1, "first missile must Impact exactly once");
        Need(cancels.GetValueOrDefault(firstId) == 0, "first missile must not Cancel if it Impacted");
        Need(applied > 0, "Impact carries AppliedDamage");
    }

    private static void PathDistinguishWaitVsStuck()
    {
        BehaviorLog.Enabled = true;
        BehaviorLog.Reset();
        try
        {
            var m = New();
            m.Spawn("eco.dropoff", 0, new(4500, 2000));
            var dock = m.Spawn("map.dock", -1, new(6000, 1800));
            dock.Supplies = config.Role("eco.chinook").CargoCapacity * 2;
            var a = m.Spawn("eco.chinook", 0, new(6000, 1800));
            var b = m.Spawn("eco.chinook", 0, new(6000, 1800));
            m.Step();
            Need(dock.LoadingId == a.Id && b.Activity == EntityActivity.Waiting, "second gatherer waits for exclusive dock loader");
            Steps(m, Math.Max(12, config.Rules.PathReplanTicks * 3) + 4);
            Need(b.Activity is EntityActivity.Waiting or EntityActivity.Loading or EntityActivity.Gathering or EntityActivity.Returning, "waiter stays in a legitimate gather state");
            Need(!BehaviorLog.Diagnostics.Any(d => d.EntityId == b.Id && d.Classification == MovementClassification.Stuck), "dock wait must not be classified stuck");
            Need(BehaviorLog.Diagnostics.Any(d => d.EntityId == b.Id && d.Classification == MovementClassification.Wait), "dock wait emits MovementClassification.Wait");

            BehaviorLog.Reset();
            var blocked = New();
            var dozer = blocked.Spawn("build.dozer", 0, new(6000, 2000));
            const int n = 12, ring = 420;
            for (int i = 0; i < n; i++)
            {
                double ang = i * Math.PI * 2 / n;
                blocked.Spawn("map.dock", -1, new(dozer.Pos.X + (int)Math.Round(Math.Cos(ang) * ring), dozer.Pos.Z + (int)Math.Round(Math.Sin(ang) * ring)));
            }
            Send(blocked, OrderKind.Move, dozer, pos: new(9000, 2000));
            Steps(blocked, Math.Max(12, config.Rules.PathReplanTicks * 3) + 8);
            Need(Dist2(dozer.Pos, new(9000, 2000)) > 2000L * 2000, "enclosed dozer must not escape through building walls");
            Need(!BehaviorLog.Diagnostics.Any(d => d.EntityId == dozer.Id && d.Classification == MovementClassification.Stuck), "PathWait / enclosed yield is not stuck");
            Need(BehaviorLog.WaitEvents > 0 || dozer.PathWait > 0 || dozer.Activity != EntityActivity.Moving, "enclosed mover is waiting or has given up, not a stuck gate");
        }
        finally
        {
            BehaviorLog.Enabled = false;
            BehaviorLog.Reset();
        }
    }

    private static int DistRoot(WorldPoint a, WorldPoint b)
    {
        long n = Dist2(a, b);
        int root = 0;
        while ((long)(root + 1) * (root + 1) <= n) root++;
        return root;
    }
}
