using System;
using System.IO;
using System.Linq;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

/// <summary>
/// Headless 1vAI run with behavior logging. Does not change the default proof suite.
/// Invoke: bash tools/audit.sh
/// </summary>
public static class BehaviorAudit
{
    public static void Run(string configPath)
    {
        BehaviorLog.Enabled = true;
        BehaviorLog.Reset();
        var c = GameConfig.Load(configPath);
        IMatch match = new MatchFactory().Create(c, c.CreateSetup());
        int buildStage = 0, trained = 0;
        var buildings = new[] { ("power.fusion", new WorldPoint(3300, 4500)), ("eco.dropoff", new WorldPoint(2300, 2800)), ("prod.barracks", new WorldPoint(1400, 2700)) };
        for (var i = 0; i < c.Rules.TickRate * 300; i++)
        {
            var view = match.Snapshot(0);
            if (view.Phase == MatchPhase.Finished) break;
            if (match.Tick % c.Ai.ThinkTicks == 0)
            {
                var dozer = view.Entities.FirstOrDefault(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer" && e.Activity == EntityActivity.Idle);
                if (buildStage < buildings.Length && dozer != null)
                {
                    var item = buildings[buildStage];
                    if (view.Entities.Any(e => e.OwnerSlot == 0 && e.RoleId == item.Item1 && e.Completed)) buildStage++;
                    else match.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: item.Item2, ProductId: item.Item1));
                }
                var barracks = view.Entities.FirstOrDefault(e => e.OwnerSlot == 0 && e.RoleId == "prod.barracks" && e.Completed && e.Queue.Length == 0);
                if (barracks != null && trained < c.Ai.AttackGroupSize)
                    if (match.Submit(new MatchOrder(0, OrderKind.Queue, new[] { barracks.Id }, ProductId: trained % 2 == 0 ? "inf.rifle" : "inf.rocket")).Accepted) trained++;
                foreach (var soldier in view.Entities.Where(e => e.OwnerSlot == 0 && e.Activity == EntityActivity.Idle && c.Role(e.RoleId).Damage > 0 && !c.Role(e.RoleId).IsBuilding))
                    match.Submit(new MatchOrder(0, OrderKind.Guard, new[] { soldier.Id }, Position: c.Map.Starts.Single(s => s.Slot == 0).Command));
            }
            match.Step();
        }
        var end = match.Snapshot(0);
        Console.WriteLine($"BEHAVIOR_AUDIT tick={match.Tick} phase={end.Phase} winners={string.Join(',', end.WinningSlots)}");
        Console.Write(BehaviorLog.Report());
        string dest = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
        Directory.CreateDirectory(dest);
        string path = Path.Combine(dest, "behavior-audit.txt");
        File.WriteAllText(path, $"tick={match.Tick} phase={end.Phase}\n" + BehaviorLog.Report());
        Console.WriteLine("BEHAVIOR_AUDIT_FILE " + path);
        BehaviorLog.Enabled = false;
        BehaviorLog.Reset();
    }
}
