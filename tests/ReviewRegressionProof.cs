using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

public static class ReviewRegressionProof
{
    private static GameConfig config = null!;
    public static void Run(string path)
    {
        config = GameConfig.Load(path);
        var failures = new List<string>();
        foreach (var test in new (string Name, Action Run)[] {
            ("builder clears its future footprint and can leave", ConstructionAccess),
            ("blocked paths cannot repair, capture, enter or build remotely", RemoteInteractions),
            ("owned occupied garrisons prevent elimination", OccupiedGarrison),
            ("powered defenses execute attack and force-attack orders", DefenseOrders),
            ("stopped cargo can be delivered after every dock empties", StrandedCargo),
            ("unload stays within local reach of the carrier", LocalUnload),
            ("ghost rejects a mobile unit on the pad", GhostOccupancy),
            ("new build leaves the previous scaffold", AbandonedScaffold),
            ("placed facing aims default rally", FacingRally) })
        {
            try { test.Run(); Console.WriteLine("PASS review regression: " + test.Name); }
            catch (InvalidOperationException e) { failures.Add(test.Name + ": " + e.Message); Console.WriteLine("FAIL review regression: " + failures[^1]); }
        }
        if (failures.Count != 0) throw new InvalidOperationException(string.Join("\n", failures));
    }
    private static void Need(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static Match New(bool enclosed = false, bool clutter = false)
    {
        var walls = enclosed ? new[] { new TerrainRect(40, 10, 8, 1, "ter.unbuildable"), new TerrainRect(40, 17, 8, 1, "ter.unbuildable"), new TerrainRect(40, 11, 1, 6, "ter.unbuildable"), new TerrainRect(47, 11, 1, 6, "ter.unbuildable") } : clutter ? config.Map.Terrain : Array.Empty<TerrainRect>();
        var c = config with { Map = config.Map with { Objects = clutter ? config.Map.Objects : Array.Empty<MapObjectConfig>(), Terrain = walls } };
        var setup = c.CreateSetup() with { Fog = false, Slots = c.DefaultSlots.Select(s => s.Occupant == Occupant.AI ? s with { Occupant = Occupant.Player } : s).ToArray() };
        return (Match)new MatchFactory().Create(c, setup);
    }
    private static void Send(Match m, OrderKind kind, Match.Body actor, int target = 0, WorldPoint position = default, string product = "")
    {
        var receipt = m.Submit(new MatchOrder(actor.Owner, kind, new[] { actor.Id }, target, position, product));
        Need(receipt.Accepted, kind + " rejected: " + receipt.Reason);
    }
    private static void Step(Match m, int count) { for (var n = 0; n < count; n++) m.Step(); }
    private static Match.Body Command(Match m, int slot) => m.Bodies.Values.Single(b => b.Owner == slot && b.RoleId == "prod.command");
    private static long Distance2(WorldPoint a, WorldPoint b) => Match.Distance2(a, b);
    private static void ConstructionAccess()
    {
        var m = New(); var dozer = m.Bodies.Values.Single(b => b.Owner == 0 && b.RoleId == "build.dozer"); var site = dozer.Pos;
        Send(m, OrderKind.Build, dozer, position: site, product: "power.fusion");
        Step(m, config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 10);
        var fusion = m.Bodies.Values.SingleOrDefault(b => b.Owner == 0 && b.RoleId == "power.fusion" && b.Complete);
        Need(fusion != null, "A legal site underneath the dozer should be built after the dozer steps aside.");
        var clearance = config.Role("power.fusion").Radius + config.Role("build.dozer").Radius;
        Need(Distance2(dozer.Pos, site) > (long)clearance * clearance, "Dozer remains embedded in the completed footprint.");
        var destination = new WorldPoint(3600, 4500); Send(m, OrderKind.Move, dozer, position: destination); Step(m, 200);
        Need(Distance2(dozer.Pos, destination) <= (long)config.Role("build.dozer").Radius * config.Role("build.dozer").Radius, "Dozer cannot leave after construction.");
    }
    private static void RemoteInteractions()
    {
        var m = New(true); var dozer = m.Spawn("build.dozer", 0, new(4400, 1400)); var repair = m.Spawn("power.fusion", 0, new(6500, 1400)); repair.Hp -= config.Role("armor.basic").Damage;
        int hp = repair.Hp, money = m.Snapshot(0).Player.Money; m.Step();
        Send(m, OrderKind.Repair, dozer, repair.Id); Step(m, 100);
        Need(repair.Hp == hp && m.Snapshot(0).Player.Money == money, "A trapped dozer repaired outside interaction range.");
        Send(m, OrderKind.Build, dozer, position: new(6500, 2500), product: "prod.barracks"); Step(m, config.Role("prod.barracks").BuildTicks + 100);
        Need(!m.Bodies.Values.Any(b => b.RoleId == "prod.barracks") && m.Snapshot(0).Player.Money == money, "A blocked builder created a remote site.");
        m = New(true); var rifle = m.Spawn("inf.rifle", 0, new(4400, 1400)); var transport = m.Spawn("eco.chinook", 0, new(6500, 1400)); transport.AutoGather = false; m.Step();
        Send(m, OrderKind.Enter, rifle, transport.Id); Step(m, 100); Need(rifle.ContainerId == 0 && transport.Occupants.Count == 0, "Enclosed infantry remotely entered a transport.");
        var capturable = m.Spawn("power.fusion", 1, new(6500, 2500)); m.Players[0].Upgrades.Add("up.capture");
        Send(m, OrderKind.Capture, rifle, capturable.Id); Step(m, config.Rules.CaptureTicks + 100);
        Need(capturable.Owner == 1 && rifle.CaptureLeft == 0, "Blocked infantry started or completed a remote capture.");
    }
    private static void OccupiedGarrison()
    {
        var m = New(); var box = m.Spawn("map.garrison", -1, new(6000, 2000)); var rifle = m.Spawn("inf.rifle", 1, new(5700, 2000)); m.Step();
        Send(m, OrderKind.Enter, rifle, box.Id); m.Step(); Need(box.Owner == 1, "Fixture entry failed.");
        m.Destroy(Command(m, 1), null); m.Step();
        Need(m.Snapshot(1).Phase == MatchPhase.Running && !m.Snapshot(1).Player.Eliminated, "Owned garrison was excluded from surviving buildings.");
        m.Destroy(box, null); m.Step(); Need(m.Snapshot(0).WinningSlots.Contains(0), "Destroyed final owned garrison must resolve the loss.");
        m = New(); m.Spawn("map.garrison", -1, new(6000, 2000)); m.Destroy(Command(m, 1), null); m.Step();
        Need(m.Snapshot(1).Player.Eliminated, "Unoccupied neutral garrison must not protect a player.");
    }
    private static void DefenseOrders()
    {
        var m = New(); var fusion = m.Spawn("power.fusion", 0, new(5000, 2000)); var turret = m.Spawn("def.patriot", 0, new(6000, 2000)); var friend = m.Spawn("build.dozer", 0, new(7000, 2000)); m.Step();
        Send(m, OrderKind.ForceAttack, turret, friend.Id); m.Step();
        Need(m.Snapshot(0).Projectiles.Any(p => p.SourceId == turret.Id && p.TargetId == friend.Id), "Accepted turret force attack did not launch.");
        Step(m, 15); Need(friend.Hp < m.MaxHp(friend), "Turret force attack cannot hit a friendly target.");
        Send(m, OrderKind.Stop, turret); m.Step(); Need(turret.Actions.Count == 0, "Defense stop must cancel its explicit target.");
        var close = m.Spawn("eco.chinook", 1, new(6900, 2300)); close.AutoGather = false;
        var selected = m.Spawn("eco.chinook", 1, new(7300, 2000)); selected.AutoGather = false;
        Send(m, OrderKind.Attack, turret, selected.Id); Step(m, config.Role("def.patriot").AttackCooldownTicks + 20);
        Need(selected.Hp < m.MaxHp(selected) && close.Hp == m.MaxHp(close), "Defense failed to prioritize the explicit hostile target.");
        m.Destroy(fusion, null); Step(m, 30); var selectedHp = selected.Hp;
        Send(m, OrderKind.ForceAttack, turret, friend.Id); Step(m, config.Role("def.patriot").AttackCooldownTicks + 20);
        Need(!m.Snapshot(0).Projectiles.Any(p => p.SourceId == turret.Id) && selected.Hp == selectedHp, "Brownout must stop explicit defense fire.");
    }
    private static void StrandedCargo()
    {
        foreach (var manualMove in new[] { false, true })
        {
            var m = New(); var dropoff = m.Spawn("eco.dropoff", 0, new(4500, 2000)); var dock = m.Spawn("map.dock", -1, new(6000, 2000)); dock.Supplies = config.Role("eco.chinook").CargoCapacity;
            var gatherer = m.Spawn("eco.chinook", 0, dock.Pos); Step(m, config.Rules.GatherLoadTicks);
            Need(gatherer.Cargo > 0 && dock.Supplies == 0, "Fixture must really load the final supply.");
            Send(m, OrderKind.Stop, gatherer); m.Step(); var stopped = gatherer.Pos; Step(m, 10); Need(gatherer.Pos == stopped && gatherer.Cargo > 0, "Stop must pause the loaded gatherer.");
            if (manualMove) Send(m, OrderKind.Move, gatherer, position: dropoff.Pos);
            else Send(m, OrderKind.Gather, gatherer, dock.Id);
            Step(m, 200);
            Need(gatherer.Cargo == 0 && m.Snapshot(0).Player.Money == config.Rules.StartingCash + config.Role("eco.chinook").CargoCapacity, "Final cargo cannot be delivered after stopping.");
            var infantry = m.Spawn("inf.rifle", 0, new(4000, 2000)); m.Step();
            Need(m.Submit(new MatchOrder(0, OrderKind.Enter, new[] { infantry.Id }, gatherer.Id)).Accepted, "Delivered gatherer must become available as infantry transport.");
        }
    }
    private static void LocalUnload()
    {
        var m = New(clutter: true);
        var carrier = m.Spawn("eco.chinook", 0, new(4000, 5000)); carrier.AutoGather = false;
        var rifle = m.Spawn("inf.rifle", 0, new(3700, 5000)); m.Step();
        Send(m, OrderKind.Enter, rifle, carrier.Id); m.Step();
        Need(rifle.ContainerId == carrier.Id, "Fixture failed to board.");
        carrier.Pos = rifle.Pos = new(5150, 2300);
        Send(m, OrderKind.Exit, carrier, position: new(5150, 0)); m.Step();
        int reach = config.Rules.InteractionRange + config.Role("eco.chinook").Radius + config.Role("inf.rifle").Radius + config.Map.CellSize;
        Need(Distance2(rifle.Pos, carrier.Pos) <= (long)reach * reach, "Unload teleported beyond local reach of the carrier.");
    }
    private static void GhostOccupancy()
    {
        var m = New(); var dozer = m.Bodies.Values.Single(b => b.Owner == 0 && b.RoleId == "build.dozer");
        var pad = new WorldPoint(5000, 4500); var unit = m.Spawn("inf.rifle", 0, pad); m.Step();
        Need(!m.CanPlace(0, dozer.Id, "power.fusion", pad).Allowed, "Ghost must reject a mobile unit standing on the pad.");
        unit.Pos = new(7000, 4500); m.Step();
        Need(m.CanPlace(0, dozer.Id, "power.fusion", pad).Allowed, "Ghost must allow the pad after the unit leaves.");
        Need(m.CanPlace(0, dozer.Id, "power.fusion", dozer.Pos).Allowed, "Ghost must ignore the builder that will step off.");
    }
    private static void AbandonedScaffold()
    {
        var m = New(); var dozer = m.Bodies.Values.Single(b => b.Owner == 0 && b.RoleId == "build.dozer");
        var pad = new WorldPoint(dozer.Pos.X + 800, dozer.Pos.Z);
        Send(m, OrderKind.Build, dozer, position: pad, product: "power.fusion");
        Step(m, 50);
        var site = m.Bodies.Values.Single(b => b.RoleId == "power.fusion" && b.Owner == 0);
        Need(!site.Complete, "Fixture must still be building.");
        Send(m, OrderKind.Build, dozer, position: new(6500, 4500), product: "prod.barracks");
        m.Step();
        Need(m.Bodies.ContainsKey(site.Id) && !site.Complete && site.BuilderId == 0, "A new Build must leave the unfinished site.");
        Need(dozer.ConstructionId != site.Id, "Dozer must detach from the abandoned site.");
        Send(m, OrderKind.Stop, dozer);
        m.Step();
        Need(m.Bodies.ContainsKey(site.Id), "Stop on a detached dozer must not delete the abandoned scaffold.");
        Send(m, OrderKind.Repair, dozer, site.Id);
        Step(m, config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 8);
        Need(site.Complete, "A dozer must be able to finish an abandoned site.");
    }
    private static void FacingRally()
    {
        var m = New(); var barracks = m.Spawn("prod.barracks", 0, new(6000, 3000));
        barracks.Facing = 90; barracks.Rally = m.FrontOf(barracks); m.Step();
        Need(barracks.Rally.X > barracks.Pos.X && Math.Abs(barracks.Rally.Z - barracks.Pos.Z) < config.Role("prod.barracks").Radius, "Facing 90 must put rally toward +X.");
        var dozer = m.Bodies.Values.Single(b => b.Owner == 0 && b.RoleId == "build.dozer");
        Need(m.Submit(new MatchOrder(0, OrderKind.Build, new[] { dozer.Id }, Position: new(dozer.Pos.X + 900, dozer.Pos.Z), ProductId: "prod.barracks", Facing: 180)).Accepted, "Faced barracks must place.");
        Step(m, config.Role("prod.barracks").BuildTicks + config.Rules.TickRate * 12);
        var second = m.Bodies.Values.Single(b => b.RoleId == "prod.barracks" && b.Id != barracks.Id && b.Complete);
        Need(second.Facing == 180 && second.Rally.Z < second.Pos.Z, "Released facing 180 must put rally toward -Z.");
    }
}
