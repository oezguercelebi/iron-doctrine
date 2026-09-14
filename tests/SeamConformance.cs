using System;
using System.Linq;
using System.Text.Json;
using IronDoctrine.Contracts;

namespace IronDoctrine.Proofs;

// Lead adopts unchanged as a frozen seam test. Invoked from executable harness, not an extra dependency.
public static class SeamConformance
{
    public static void Run(IMatchFactory factory, string configPath)
    {
        var config = GameConfig.Load(configPath);
        var setup = config.CreateSetup();
        var first = factory.Create(config, setup);
        var second = factory.Create(config, setup);
        Require(first.Setup.Slots.Length == CatalogIds.MaxSlots, "eight slots retained");
        Require(CatalogIds.Loadouts.Contains("veil.stealth") && CatalogIds.Prefixes.Contains("ent."), "future id space retained");
        var initial = first.Snapshot(0);
        var initialJson = JsonSerializer.Serialize(initial);
        var own = initial.Entities.Where(e => e.OwnerSlot == 0).ToArray();
        Require(own.Length == 2 && own.Any(e => e.RoleId == "prod.command") && own.Any(e => e.RoleId == "build.dozer"), "only command and builder spawn");
        Require(!initial.Entities.Any(e => e.OwnerSlot == 1), "enemy start hidden behind shroud");
        Require(initial.Visibility.Length == config.Map.WidthCells * config.Map.HeightCells, "fog grid shape");
        Require(initial.Player.Radar && initial.Player.Money == config.Rules.StartingCash, "initial private HUD");
        var dozer = own.Single(e => e.RoleId == "build.dozer");
        Require(!first.Submit(new MatchOrder(1, OrderKind.Stop, new[] { dozer.Id })).Accepted, "foreign unit orders rejected");
        // Invalid submission must leave deterministic state untouched.
        Require(first.StateHash() == second.StateHash(), "rejected order has no sim effect");
        var spot = new WorldPoint(3300, 4500); // Original map fixture, not gameplay tuning.
        // Terrain walk vs build for ter.unbuildable is owned by AcceptanceContracts (catalog TERRAIN.md), not this seam.
        Require(first.CanPlace(0, dozer.Id, "power.fusion", spot).Allowed, "legal construction place");
        var order = new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: spot, ProductId: "power.fusion");
        Require(first.Submit(order).Accepted && second.Submit(order).Accepted, "build accepted through seam");
        var duration = config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 10;
        for (var i = 0; i < duration; i++) { first.Step(); second.Step(); }
        Require(first.StateHash() == second.StateHash(), "same seed plus same ordered inputs is deterministic");
        var built = first.Snapshot(0);
        Require(built.Entities.Any(e => e.RoleId == "power.fusion" && e.OwnerSlot == 0 && e.Completed), "orders advance real construction");
        Require(built.Player.Money == config.Rules.StartingCash - config.Role("power.fusion").Cost, "construction debits owner once");
        Require(JsonSerializer.Serialize(initial) == initialJson, "prior snapshots remain stable");
        first.SetPaused(true);
        var pausedTick = first.Tick;
        var pausedHash = first.StateHash();
        first.Step();
        Require(first.Tick == pausedTick && first.StateHash() == pausedHash && first.Snapshot(0).Paused, "pause freezes full sim");
        first.SetPaused(false);
        Require(first.Submit(new MatchOrder(0, OrderKind.Resign, Array.Empty<int>())).Accepted, "resign accepted");
        first.Step();
        var end = first.Snapshot(0);
        Require(end.Phase == MatchPhase.Finished && end.EliminatedSlots.Contains(0) && end.WinningSlots.Contains(1), "resign resolves outcome");
        Console.WriteLine("PASS seam: ids, slots, fog, private HUD, ownership, construction, replay, snapshots, pause, result");
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Seam failure: " + message);
    }
}
