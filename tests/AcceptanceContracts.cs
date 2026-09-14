using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Proofs;

public static class AcceptanceContracts
{
    public static void Run(IMatchFactory factory, string configPath)
    {
        var config = GameConfig.Load(configPath);
        var failures = new List<string>();
        Check(failures, "map helpers catalog TERRAIN.md", () => MapHelpers(config));
        Check(failures, ScenarioIds.PathGroundTraversesUnbuildable, () => PathGroundTraversesUnbuildable(factory, config));
        Check(failures, ScenarioIds.CombatInstantTracerTiedToShot, () => CombatInstantTraces(factory, config));
        Check(failures, "scenario harness stubs", ScenarioHarnessStubs);
        if (failures.Count != 0) throw new InvalidOperationException(string.Join("\n", failures));
        Console.WriteLine("PASS acceptance: terrain walk/build, combat traces, scenario stubs");
    }

    private static void Check(List<string> failures, string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("PASS acceptance: " + name);
        }
        catch (InvalidOperationException e)
        {
            failures.Add(e.Message);
            Console.WriteLine("FAIL acceptance: " + e.Message);
        }
    }

    private static void Need(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static IMatch Solo(IMatchFactory factory, GameConfig config)
    {
        var setup = config.CreateSetup() with
        {
            Fog = false,
            Slots = config.DefaultSlots.Select(s => s.Index == 0 ? s : s with { Occupant = Occupant.Closed }).ToArray()
        };
        return factory.Create(config, setup);
    }

    private static (int X, int Z) CellOf(MapConfig map, WorldPoint point) => (point.X / map.CellSize, point.Z / map.CellSize);

    private static bool OnUnbuildable(MapConfig map, WorldPoint point)
    {
        var (x, z) = CellOf(map, point);
        return map.InMap(x, z) && map.TerrainAt(x, z) == "ter.unbuildable";
    }

    private static WorldPoint CellCenter(MapConfig map, int cellX, int cellZ) =>
        new(cellX * map.CellSize + map.CellSize / 2, cellZ * map.CellSize + map.CellSize / 2);

    private static void Until(IMatch match, Func<MatchSnapshot, bool> done, int limit, string message)
    {
        for (var i = 0; i < limit && !done(match.Snapshot(0)); i++) match.Step();
        Need(done(match.Snapshot(0)), message);
    }

    private static void MapHelpers(GameConfig config)
    {
        var map = config.Map;
        Need(map.GroundWalkable("ter.unbuildable") && map.AirWalkable("ter.unbuildable") && !map.CanBuildOn("ter.unbuildable"),
            "TERRAIN.md ter.unbuildable: ground yes, air yes, build no");
        Need(map.GroundWalkable("ter.ground") && map.CanBuildOn("ter.ground") && map.GroundWalkable("ter.start") && map.CanBuildOn("ter.start"),
            "TERRAIN.md ter.ground / ter.start walk and build");
        Need(!map.GroundWalkable("ter.block") && !map.AirWalkable("ter.block") && !map.CanBuildOn("ter.block"),
            "TERRAIN.md ter.block is none");
        Need(!map.GroundWalkable("ter.cliff") && map.AirWalkable("ter.cliff") && !map.CanBuildOn("ter.cliff"),
            "TERRAIN.md ter.cliff: ground no, air yes, build no");
        Need(!map.InMap(-1, 0) && !map.InMap(map.WidthCells, 0) && !map.InMap(0, map.HeightCells),
            "OOB is not a map cell");
        Need(!map.InMap(new WorldPoint(-1, 0)) && !map.InMap(new WorldPoint(map.WidthCells * map.CellSize, 0)),
            "OOB world point is not in map");
        Need(map.TerrainAt(-1, 0) == "ter.block" && map.TerrainAt(map.WidthCells, 0) == "ter.block",
            "OOB TerrainAt is ter.block, not ter.unbuildable");
        Need(map.Terrain.Any(r => r.TerrainId == "ter.unbuildable"),
            "slice map must include a real ter.unbuildable rectangle");
    }

    private static void PathGroundTraversesUnbuildable(IMatchFactory factory, GameConfig config)
    {
        var map = config.Map;
        var rect = map.Terrain.First(r => r.TerrainId == "ter.unbuildable");
        var origin = map.Starts.Single(s => s.Slot == 0).Builder;
        int bestX = rect.X + rect.Width / 2, bestZ = rect.Z + rect.Height / 2, best = int.MaxValue;
        var (ox, oz) = CellOf(map, origin);
        foreach (var r in map.Terrain.Where(t => t.TerrainId == "ter.unbuildable"))
            for (var z = r.Z; z < r.Z + r.Height; z++)
                for (var x = r.X; x < r.X + r.Width; x++)
                {
                    if (!map.InMap(x, z) || map.TerrainAt(x, z) != "ter.unbuildable") continue;
                    int d = (x - ox) * (x - ox) + (z - oz) * (z - oz);
                    if (d < best) { best = d; bestX = x; bestZ = z; }
                }
        Need(map.InMap(bestX, bestZ) && map.TerrainAt(bestX, bestZ) == "ter.unbuildable" && map.GroundWalkable(map.TerrainAt(bestX, bestZ)),
            "target cell must be catalog ter.unbuildable ground");
        var target = CellCenter(map, bestX, bestZ);
        var match = Solo(factory, config);
        var dozer = match.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        var place = match.CanPlace(0, dozer.Id, "power.fusion", target);
        Need(!place.Allowed, "TERRAIN.md ter.unbuildable build=no: CanPlace must deny power.fusion on that cell");
        var build = match.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: target, ProductId: "power.fusion"));
        Need(!build.Accepted, "TERRAIN.md ter.unbuildable build=no: Build must be rejected");
        // Ghost occupancy is provisional (Q2): CanPlace blocks visible units. Hidden enemy occupancy must not
        // leak via reason codes. Full fog-safe occupancy is not in this freeze; keep current CanPlace behavior.
        var move = match.Submit(new MatchOrder(0, OrderKind.Move, new[] { dozer.Id }, Position: target));
        Need(move.Accepted, "Move onto catalog-walkable ter.unbuildable must be accepted");
        bool traversed = false;
        int deadline = Math.Max(800, config.Rules.TickRate * 40);
        for (var i = 0; i < deadline; i++)
        {
            match.Step();
            var body = match.Snapshot(0).Entities.Single(e => e.Id == dozer.Id);
            if (OnUnbuildable(map, body.Position)) { traversed = true; break; }
        }
        Need(traversed,
            "Acceptance path.ground_traverses_unbuildable: TERRAIN.md ter.unbuildable is ground-walkable; unit never occupied or traversed that cell (catalog: ground yes, build no).");
    }

    private static void CombatInstantTraces(IMatchFactory factory, GameConfig config)
    {
        Need(config.Role("inf.rifle").DeliveryId == "del.instant", "inf.rifle is del.instant");
        // Same public Build seam SeamConformance already exercises; fog off so the garrison is a known target.
        var match = factory.Create(config, config.CreateSetup() with { Fog = false });
        var dozer = match.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        var fusionAt = new WorldPoint(3300, 4500);
        var barracksAt = new WorldPoint(1400, 2700);
        Need(match.CanPlace(0, dozer.Id, "power.fusion", fusionAt).Allowed, "fusion pad must be legal for arrangement");
        Need(match.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: fusionAt, ProductId: "power.fusion")).Accepted, "fusion build accepted");
        var fusionWait = config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 10;
        Until(match, s => s.Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == "power.fusion" && e.Completed), fusionWait, "fusion must complete");
        dozer = match.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        Need(match.CanPlace(0, dozer.Id, "prod.barracks", barracksAt).Allowed, "barracks pad must be legal for arrangement");
        Need(match.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: barracksAt, ProductId: "prod.barracks")).Accepted, "barracks build accepted");
        Until(match, s => s.Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == "prod.barracks" && e.Completed), config.Role("prod.barracks").BuildTicks + config.Rules.TickRate * 20, "barracks must complete");
        var barracks = match.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "prod.barracks" && e.Completed);
        Need(match.Submit(new MatchOrder(0, OrderKind.Queue, new[] { barracks.Id }, ProductId: "inf.rifle")).Accepted, "rifle queued");
        Until(match, s => s.Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == "inf.rifle"), config.Role("inf.rifle").BuildTicks + config.Rules.TickRate * 5, "rifle must spawn");
        var rifle = match.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "inf.rifle");
        var garrison = match.Snapshot(0).Entities.Where(e => e.RoleId == "map.garrison" && !e.IsRemembered).OrderBy(e => Dist2(e.Position, rifle.Position)).FirstOrDefault();
        Need(garrison != null, "visible garrison required for del.instant arrangement");
        int startHp = garrison!.Hp;
        Need(match.Submit(new MatchOrder(0, OrderKind.ForceAttack, new[] { rifle.Id }, TargetId: garrison.Id)).Accepted, "force-attack garrison accepted");
        MatchSnapshot? hit = null;
        for (var i = 0; i < config.Rules.TickRate * 40; i++)
        {
            match.Step();
            var snap = match.Snapshot(0);
            var target = snap.Entities.FirstOrDefault(e => e.Id == garrison.Id);
            if (target != null && target.Hp < startHp) { hit = snap; break; }
        }
        Need(hit != null, "legal del.instant engagement (inf.rifle vs map.garrison) must apply damage before traces are judged");
        var traces = hit!.CombatTraces;
        bool paired = traces.Any(launch => launch.Phase == CombatTracePhase.Launch && launch.DeliveryId == "del.instant"
            && traces.Any(impact => impact.Phase == CombatTracePhase.Impact && impact.Id == launch.Id));
        Need(traces.Length > 0,
            "Acceptance combat traces: MatchSnapshot.CombatTraces empty after del.instant engagement; Launch/Impact sharing an Id is required (diagnostic traces, not catalog rows).");
        Need(paired, "Acceptance combat traces: del.instant Launch and Impact must share an Id.");
    }

    private static void ScenarioHarnessStubs()
    {
        Need(ScenarioHarness.GuiWidth == 1440 && ScenarioHarness.GuiHeight == 900, "GUI proof size 1440×900");
        var ids = ScenarioHarness.ListIds();
        Need(ids.Contains(ScenarioIds.PathGroundTraversesUnbuildable) && ids.Contains(ScenarioIds.CombatInstantTracerTiedToShot),
            "named scenario ids are defined");
        Need(ScenarioHarness.TryLoad(ScenarioIds.PathGroundTraversesUnbuildable, out var path) && path != null && path.Id == ScenarioIds.PathGroundTraversesUnbuildable,
            "TryLoad returns the traverse scenario stub");
        Need(path!.CatalogRule.Contains("ter.unbuildable", StringComparison.Ordinal) && path.Assertions.Forbid.Any(f => f.Contains("impassable", StringComparison.Ordinal)),
            "traverse stub carries catalog forbid");
        Need(ScenarioHarness.TryLoad(ScenarioIds.BuildGhostMatchesArrivalOccupancy, out var ghost) && ghost != null
            && ghost.CatalogRule.Contains("provisional", StringComparison.OrdinalIgnoreCase),
            "ghost occupancy marked provisional");
        Need(ScenarioHarness.TryLoad(ScenarioIds.PathDistinguishWaitVsStuck, out var wait) && wait != null
            && wait.Assertions.Forbid.Any(f => f.Contains("raw audit", StringComparison.Ordinal)),
            "wait vs stuck is not a raw-count full-match gate");
        Need(ReplayBundle.InputsJsonl == "inputs.jsonl" && ReplayBundle.OutcomeJson == "outcome.json"
            && ReplayBundle.CheckpointsJsonl == "checkpoints.jsonl",
            "replay bundle field names frozen");
        Need(!ScenarioHarness.TryLoad("not.a.scenario", out _), "unknown scenario id does not load");
    }

    private static long Dist2(WorldPoint a, WorldPoint b)
    {
        long dx = a.X - b.X, dz = a.Z - b.Z;
        return dx * dx + dz * dz;
    }
}
