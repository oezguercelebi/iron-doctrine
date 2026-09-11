using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

public static class FullMatchProof
{
    // This proof has no Match cast, fixture mutation or privileged controller. Both sides start normally.
    public static void Run(string configPath)
    {
        var c = GameConfig.Load(configPath);
        IMatch first = new MatchFactory().Create(c, c.CreateSetup());
        IMatch replay = new MatchFactory().Create(c, c.CreateSetup());
        var aiRoles = new HashSet<string>(StringComparer.Ordinal);
        int buildStage = 0, trained = 0, submitted = 0;
        var buildings = new[] { ("power.fusion", new WorldPoint(3300, 4500)), ("eco.dropoff", new WorldPoint(2300, 2800)), ("prod.barracks", new WorldPoint(1400, 2700)) };
        var started = DateTime.UtcNow;
        for (var iteration = 0; iteration < c.Rules.TickRate * 300; iteration++)
        {
            var view = first.Snapshot(0);
            if (view.Phase == MatchPhase.Finished) break;
            bool Send(MatchOrder order)
            {
                var a = first.Submit(order); var b = replay.Submit(order);
                Require(a == b, "replay must accept identical commands");
                if (a.Accepted) submitted++;
                return a.Accepted;
            }
            if (first.Tick % c.Ai.ThinkTicks == 0)
            {
                var dozer = view.Entities.FirstOrDefault(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer" && e.Activity == EntityActivity.Idle);
                if (buildStage < buildings.Length && dozer != null)
                {
                    var item = buildings[buildStage];
                    if (view.Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == item.Item1 && e.Completed)) buildStage++;
                    else Send(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: item.Item2, ProductId: item.Item1));
                }
                var barracks = view.Entities.FirstOrDefault(e => e.OwnerSlot == 0 && e.RoleId == "prod.barracks" && e.Completed && e.Queue.Length == 0);
                if (barracks != null && trained < c.Ai.AttackGroupSize)
                    if (Send(new MatchOrder(0, OrderKind.Queue, new[] { barracks.Id }, ProductId: trained % 2 == 0 ? "inf.rifle" : "inf.rocket"))) trained++;
                foreach (var soldier in view.Entities.Where(e => e.OwnerSlot == 0 && e.Activity == EntityActivity.Idle && c.Role(e.RoleId).Damage > 0 && !c.Role(e.RoleId).IsBuilding))
                    Send(new MatchOrder(0, OrderKind.Guard, new[] { soldier.Id }, Position: c.Map.Starts.Single(s => s.Slot == 0).Command));
            }
            first.Step(); replay.Step();
            foreach (var role in first.Snapshot(1).Entities.Where(e => e.OwnerSlot == 1 && e.Completed).Select(e => e.RoleId)) aiRoles.Add(role);
            if (first.Tick % (c.Rules.TickRate * 5) == 0) Require(first.StateHash() == replay.StateHash(), "replay drift at tick " + first.Tick);
            if (first.Tick % (c.Rules.TickRate * 30) == 0)
            {
                var ai = first.Snapshot(1);
                Console.WriteLine($"Full match progress tick={first.Tick} playerBuildings={first.Snapshot(0).Entities.Count(e => e.OwnerSlot == 0 && c.Role(e.RoleId).IsBuilding)} aiMoney={ai.Player.Money} aiArmy={ai.Entities.Count(e => e.OwnerSlot == 1 && !c.Role(e.RoleId).IsBuilding && c.Role(e.RoleId).Damage > 0)}");
            }
        }
        var result = first.Snapshot(0);
        Require(result.Phase == MatchPhase.Finished, "Normal order-driven match must finish without a timeout victory");
        Require(result.EliminatedSlots.Contains(0) && result.WinningSlots.Contains(1), "Built-in AI must finish its attack against a defending player base");
        Require(submitted > 0 && trained > 0, "Human controller must construct and train through public orders");
        foreach (var role in new[] { "power.fusion", "eco.dropoff", "prod.barracks", "prod.factory", "def.patriot", "inf.rifle", "inf.rocket", "armor.basic", "veh.scout_gun", "eco.chinook" }) Require(aiRoles.Contains(role), "AI exercised role " + role);
        Require(first.StateHash() == replay.StateHash(), "Final replay state including AI and fog agrees");
        Console.WriteLine($"PASS full match: default human-v-AI setup, {submitted} public player orders, AI base/gather/compose/attack/defend, defeat at tick {first.Tick}, deterministic replay {first.StateHash()}, elapsed {(DateTime.UtcNow - started).TotalSeconds:F1}s");
    }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
}
