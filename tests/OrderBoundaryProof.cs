using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

public static class OrderBoundaryProof
{
    private static GameConfig config = null!;
    public static void Run(string path)
    {
        config = GameConfig.Load(path);
        var failures = new List<string>();
        foreach (var test in new (string Name, Action Run)[] {
            ("point and targetless orders reject incidental target ids without revealing state", PointPayloads),
            ("appended container and passenger exits follow the transport queue", QueuedExit),
            ("capture removes the previous owner's queued combat control", CapturedOrders) })
        {
            try { test.Run(); Console.WriteLine("PASS order boundary: " + test.Name); }
            catch (InvalidOperationException e) { failures.Add(test.Name + ": " + e.Message); Console.WriteLine("FAIL order boundary: " + failures[^1]); }
        }
        if (failures.Count > 0) throw new InvalidOperationException(string.Join("\n", failures));
    }
    private static Match New(bool fog = false)
    {
        var c = config with { Map = config.Map with { Terrain = Array.Empty<TerrainRect>(), Objects = Array.Empty<MapObjectConfig>() } };
        return (Match)new MatchFactory().Create(c, c.CreateSetup() with { Fog = fog, Slots = c.DefaultSlots.Select(s => s.Occupant == Occupant.AI ? s with { Occupant = Occupant.Player } : s).ToArray() });
    }
    private static void Need(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static void Send(Match m, MatchOrder order) { var receipt = m.Submit(order); Need(receipt.Accepted, order.Kind + " rejected: " + receipt.Reason); }
    private static void Steps(Match m, int count) { for (int n = 0; n < count; n++) m.Step(); }
    private static void PointPayloads()
    {
        var point = new WorldPoint(3300, 4500);
        foreach (var kind in new[] { OrderKind.Move, OrderKind.Waypoint, OrderKind.AttackMove, OrderKind.Build, OrderKind.Rally, OrderKind.Exit, OrderKind.Queue, OrderKind.CancelQueue, OrderKind.Sell, OrderKind.Stop, OrderKind.Resign })
        {
            var m = New(true);
            var dozer = m.Bodies.Values.Single(e => e.Owner == 0 && e.RoleId == "build.dozer");
            var command = m.Bodies.Values.Single(e => e.Owner == 0 && e.RoleId == "prod.command");
            var enemy = m.Bodies.Values.Single(e => e.Owner == 1 && e.RoleId == "prod.command");
            Match.Body actor = dozer; string product = "";
            if (kind == OrderKind.AttackMove) actor = m.Spawn("inf.rifle", 0, new(2300, 5000));
            if (kind is OrderKind.Rally or OrderKind.Queue or OrderKind.CancelQueue or OrderKind.Sell) actor = command;
            if (kind == OrderKind.Build) product = "power.fusion";
            if (kind == OrderKind.Queue) product = "build.dozer";
            if (kind == OrderKind.CancelQueue) { Send(m, new MatchOrder(0, OrderKind.Queue, new[] { command.Id }, ProductId: "build.dozer")); m.Step(); }
            if (kind == OrderKind.Exit)
            {
                actor = m.Spawn("eco.chinook", 0, new(2800, 5000)); var passenger = m.Spawn("inf.rifle", 0, new(2500, 5000)); m.Step();
                Send(m, new MatchOrder(0, OrderKind.Enter, new[] { passenger.Id }, actor.Id)); m.Step();
                Need(passenger.ContainerId == actor.Id, "Exit fixture failed to board.");
            }
            Need(!m.Snapshot(0).Entities.Any(e => e.Id == enemy.Id), "Enemy target must be unseen.");
            var ids = kind == OrderKind.Resign ? Array.Empty<int>() : new[] { actor.Id };
            var order = new MatchOrder(0, kind, ids, Position: point, ProductId: product);
            var before = m.StateHash();
            var hidden = m.Submit(order with { TargetId = enemy.Id });
            var nonexistent = m.Submit(order with { TargetId = int.MaxValue });
            Need(!hidden.Accepted && !nonexistent.Accepted && hidden.Reason == nonexistent.Reason, kind + " accepted an incidental hidden target or exposed its existence.");
            Need(m.StateHash() == before, "Rejected target payload changed state for " + kind);
            Send(m, order); m.Step();
            if (kind is OrderKind.Move or OrderKind.Waypoint or OrderKind.AttackMove or OrderKind.Build)
                Need(m.Snapshot(0).Entities.Single(e => e.Id == actor.Id).Destination == point, kind + " did not retain the requested point.");
            if (kind == OrderKind.Build)
            {
                Steps(m, config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 10);
                Need(m.Snapshot(0).Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == "power.fusion" && e.Completed && e.Position == point), "Build moved away from its validated placement ghost.");
            }
        }
        // Target-bearing actions still require real current visibility and do not turn a guessed id into a destination.
        var targetMatch = New(true); var soldier = targetMatch.Spawn("inf.rifle", 0, new(2300, 5000));
        var hiddenCommand = targetMatch.Bodies.Values.Single(e => e.Owner == 1 && e.RoleId == "prod.command");
        Need(!targetMatch.Submit(new MatchOrder(0, OrderKind.Attack, new[] { soldier.Id }, hiddenCommand.Id)).Accepted, "Attack accepted a hidden target.");
        Need(!targetMatch.Submit(new MatchOrder(0, OrderKind.ForceAttack, new[] { soldier.Id }, hiddenCommand.Id)).Accepted, "Force attack accepted a hidden target.");
    }
    private static void QueuedExit()
    {
        foreach (var passengerActor in new[] { false, true })
        {
            var m = New(); var transport = m.Spawn("eco.chinook", 0, new(6000, 2000));
            var first = m.Spawn("inf.rifle", 0, new(5700, 2000)); var second = m.Spawn("inf.rifle", 0, new(5700, 2200)); m.Step();
            Send(m, new MatchOrder(0, OrderKind.Enter, new[] { first.Id, second.Id }, transport.Id)); m.Step();
            Need(first.ContainerId == transport.Id && second.ContainerId == transport.Id, "Passengers failed to board.");
            var landing = new WorldPoint(7000, 3500);
            Send(m, new MatchOrder(0, OrderKind.Move, new[] { transport.Id }, Position: new(6500, 3000)));
            Send(m, new MatchOrder(0, OrderKind.Waypoint, new[] { transport.Id }, Position: landing));
            Send(m, new MatchOrder(0, OrderKind.Exit, new[] { passengerActor ? first.Id : transport.Id }, Position: landing, Append: true));
            if (passengerActor) Send(m, new MatchOrder(0, OrderKind.Waypoint, new[] { transport.Id }, Position: new(8000, 4000)));
            m.Step();
            Need(first.ContainerId == transport.Id && second.ContainerId == transport.Id, "Appended exit ran at the departure point.");
            for (int tick = 0; tick < 200 && first.ContainerId != 0; tick++)
            {
                if (Match.Distance2(transport.Pos, landing) > (long)config.Role("eco.chinook").Radius * config.Role("eco.chinook").Radius)
                    Need(first.ContainerId == transport.Id, "Passenger exited before the preceding move arrived.");
                m.Step();
            }
            Need(first.ContainerId == 0 && Match.Distance2(transport.Pos, landing) <= (long)config.Role("eco.chinook").Radius * config.Role("eco.chinook").Radius, "Queued exit never completed after arrival.");
            Need(passengerActor ? second.ContainerId == transport.Id : second.ContainerId == 0, "Queued exit changed which passengers were selected.");
            if (passengerActor)
            {
                Send(m, new MatchOrder(0, OrderKind.Move, new[] { transport.Id }, Position: new(7000, 5000)));
                Send(m, new MatchOrder(0, OrderKind.Exit, new[] { second.Id }, Position: new(7000, 5000), Append: true)); m.Step();
                Send(m, new MatchOrder(0, OrderKind.Stop, new[] { transport.Id })); Steps(m, 100);
                Need(second.ContainerId == transport.Id, "Stopping the transport failed to cancel its queued passenger unload.");
            }
        }
        var g = New(); var box = g.Spawn("map.garrison", -1, new(6000, 2000)); var infantry = g.Spawn("inf.rifle", 0, new(5700, 2000)); g.Step();
        Send(g, new MatchOrder(0, OrderKind.Enter, new[] { infantry.Id }, box.Id)); g.Step();
        Send(g, new MatchOrder(0, OrderKind.Exit, new[] { box.Id }, Position: new(5700, 2000), Append: true)); g.Step();
        Need(infantry.ContainerId == 0 && box.Owner == -1, "A stationary garrison must also execute its queued exit.");
    }
    private static void CapturedOrders()
    {
        var m = New(); var victim = m.Spawn("power.fusion", 0, new(5000, 2000)); m.Spawn("power.fusion", 1, new(8000, 2000));
        var turret = m.Spawn("def.patriot", 1, new(6000, 2000)); var rifle = m.Spawn("inf.rifle", 0, new(6500, 2000)); m.Players[0].Upgrades.Add("up.capture");
        Send(m, new MatchOrder(1, OrderKind.ForceAttack, new[] { turret.Id }, victim.Id));
        Send(m, new MatchOrder(0, OrderKind.Capture, new[] { rifle.Id }, turret.Id));
        for (int tick = 0; tick < config.Rules.CaptureTicks + 100 && turret.Owner == 1; tick++) m.Step();
        Need(turret.Owner == 0 && turret.Powered, "Fixture must capture the powered defense through its channel.");
        Need(turret.Actions.Count == 0 && turret.Rally == turret.Pos && turret.Destination == turret.Pos, "Capture retained old combat orders or destinations.");
        Need(!m.Submit(new MatchOrder(1, OrderKind.ForceAttack, new[] { turret.Id }, victim.Id)).Accepted, "Previous owner can still command the captured defense.");
        // Already-launched missiles may resolve; subsequent friendly fire must cease.
        Steps(m, config.Rules.TickRate * 2); int hp = victim.Hp;
        Steps(m, config.Role("def.patriot").AttackCooldownTicks * 2);
        Need(victim.Hp == hp && !m.Snapshot(0).Projectiles.Any(p => p.SourceId == turret.Id && p.TargetId == victim.Id), "Captured turret kept attacking a now-friendly building.");
    }
}
