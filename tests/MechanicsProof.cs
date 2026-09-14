using System;
using System.Linq;
using System.Text.Json;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

namespace IronDoctrine.Proofs;

public static class MechanicsProof
{
    private static GameConfig config = null!;
    public static void Run(string path)
    {
        config = GameConfig.Load(path);
        Check("construction prerequisites, loss and free gatherer", Construction);
        Check("production queues, owner costs, refund, rally and power", Production);
        Check("finite dock, exclusive loading and owner-only return", Economy);
        Check("air targeting, world missiles and damage matchups", Combat);
        Check("friendly crush and force attack", FriendlyFire);
        Check("garrison ownership, armor and destruction spill", Garrison);
        Check("scout fire-out, gatherer transport and local unload", Transport);
        Check("research, interruptible capture and original-owner refunds", Capture);
        Check("paid allied building repair and own factory repair", Repair);
        Check("kill-value rank health bonuses and delayed self-heal", Veterancy);
        Check("fog memory, redaction, defensive snapshots and hidden collision", Intel);
        Check("terrain pathing, waypoints, guard and flying traversal", Movement);
        Check("eight-slot teams, unfinished buildings and elimination", Outcome);
        Check("AI hidden-information equality, defense and depleted-dock expansion", Ai);
    }
    private static void Check(string label, Action test) { test(); Console.WriteLine("PASS mechanics: " + label); }
    private static void Need(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private static Match New(bool fog = false, bool allied = false)
    {
        var c = config with { Map = config.Map with { Objects = Array.Empty<MapObjectConfig>(), Terrain = Array.Empty<TerrainRect>() } };
        if (allied) c = c with { Map = c.Map with { MaxPlayers = 3, Starts = c.Map.Starts.Concat(new[] { new StartConfig(7, new(11000, 7000), new(10500, 7000)) }).ToArray() } };
        var s = c.CreateSetup() with { Fog = fog, Slots = c.DefaultSlots.Select(p => p.Index == 1 ? p with { Occupant = Occupant.Player, Team = allied ? 1 : 2 } : p.Index == 7 && allied ? p with { Occupant = Occupant.Player, Team = 7 } : p).ToArray() };
        return (Match)new MatchFactory().Create(c, s);
    }
    private static Match.Body Own(Match m, string role, int owner = 0) => m.Bodies.Values.First(b => b.RoleId == role && b.Owner == owner);
    private static void Send(Match m, OrderKind kind, Match.Body actor, int target = 0, string product = "", WorldPoint pos = default, bool append = false)
    {
        var r = m.Submit(new MatchOrder(actor.Owner, kind, new[] { actor.Id }, target, pos, product, append));
        Need(r.Accepted, kind + " rejected: " + r.Reason);
    }
    private static void Steps(Match m, int count) { for (int i = 0; i < count; i++) m.Step(); }
    private static void Until(Match m, Func<bool> done, int limit, string message) { for (int i = 0; i < limit && !done(); i++) m.Step(); Need(done(), message); }
    private static void Construction()
    {
        var m = New(); var dozer = Own(m, "build.dozer");
        Need(!m.CanPlace(0, dozer.Id, "prod.factory", new(3300, 4500)).Allowed, "Factory prerequisite absent");
        Need(!m.CanPlace(0, dozer.Id, "def.patriot", new(3300, 4500)).Allowed, "Defense prerequisite absent");
        Need(!m.CanPlace(0, dozer.Id, "map.garrison", new(3300, 4500)).Allowed, "Cannot build map objects");
        Send(m, OrderKind.Build, dozer, product: "power.fusion", pos: new(3300, 4500));
        Until(m, () => m.Bodies.Values.Any(b => b.RoleId == "power.fusion"), 200, "Builder must approach and construct");
        var site = Own(m, "power.fusion"); Need(!site.Complete && site.BuilderId == dozer.Id, "Site tied to builder");
        Need(m.Snapshot(0).Player.Money == config.Rules.StartingCash - config.Role("power.fusion").Cost, "Single construction debit");
        m.Hit(dozer, dozer.Hp, null);
        Need(!m.Bodies.ContainsKey(site.Id), "Builder death removes incomplete building");
        Need(m.Snapshot(0).Player.Money == config.Rules.StartingCash - config.Role("power.fusion").Cost, "No death refund");
        m = New(); dozer = Own(m, "build.dozer");
        Send(m, OrderKind.Build, dozer, product: "eco.dropoff", pos: new(3300, 4500));
        Until(m, () => m.Bodies.Values.Any(b => b.RoleId == "eco.chinook"), 400, "Built dropoff grants gatherer");
        Need(m.Bodies.Values.Count(b => b.RoleId == "eco.chinook") == 1, "Exactly one free gatherer");
        Steps(m, 100); Need(m.Bodies.Values.Count(b => b.RoleId == "eco.chinook") == 1, "Free grant cannot repeat");
    }
    private static void Production()
    {
        var m = New(); var barracks = m.Spawn("prod.barracks", 0, new(6000, 2000)); var fusion = m.Spawn("power.fusion", 0, new(5000, 2000)); m.Step();
        Need(barracks.Rally != barracks.Pos && Distance2(barracks.Rally, barracks.Pos) >= (long)config.Role("prod.barracks").Radius * config.Role("prod.barracks").Radius, "Default rally sits in front of the producer, not on its footprint");
        Need(fusion.Rally.Equals(fusion.Pos), "Non-producers do not get a rally flag offset");
        var rally = new WorldPoint(7000, 3000); Send(m, OrderKind.Rally, barracks, pos: rally);
        Send(m, OrderKind.Queue, barracks, product: "inf.rifle"); m.Step();
        Need(barracks.Queue.Count == 1 && barracks.Queue[0].Left < config.Role("inf.rifle").BuildTicks, "Production clock advances");
        Need(m.Snapshot(0).Player.Money == config.Rules.StartingCash - config.Role("inf.rifle").Cost, "Queue charges owner");
        Send(m, OrderKind.CancelQueue, barracks); m.Step(); Need(barracks.Queue.Count == 0 && m.Snapshot(0).Player.Money == config.Rules.StartingCash, "Cancel refunds paid price");
        Send(m, OrderKind.Queue, barracks, product: "inf.rocket"); m.Step();
        var remaining = barracks.Queue[0].Left;
        m.Destroy(fusion, null); m.Step();
        Need(barracks.Queue[0].Left == remaining && !m.Snapshot(0).Player.Radar && m.Snapshot(0).Player.LowPower, "Brownout pauses powered queue and radar");
        m.Spawn("power.fusion", 0, new(5000, 2000));
        Until(m, () => m.Bodies.Values.Any(b => b.RoleId == "inf.rocket" && b.Owner == 0), config.Role("inf.rocket").BuildTicks + 2, "Production resumes");
        var rocket = Own(m, "inf.rocket"); Steps(m, 10); Need(rocket.Destination == rally, "Produced unit follows rally");
        var turret = m.Spawn("def.patriot", 0, new(6000, 4500)); var air = m.Spawn("eco.chinook", 1, new(7000, 4500)); air.AutoGather = false;
        m.Destroy(Own(m, "power.fusion"), null); m.Step(); Steps(m, config.Role("def.patriot").AttackCooldownTicks);
        Need(air.Hp == m.MaxHp(air), "Unpowered defense cannot fire");
    }
    private static void Economy()
    {
        var m = New(allied: true); m.Spawn("power.fusion", 0, new(4000, 1000));
        var ownDrop = m.Spawn("eco.dropoff", 0, new(4500, 2000)); m.Spawn("eco.dropoff", 1, new(6000, 2400));
        var dock = m.Spawn("map.dock", -1, new(6000, 1800)); dock.Supplies = config.Role("eco.chinook").CargoCapacity * 2;
        var a = m.Spawn("eco.chinook", 0, new(6000, 1800)); var b = m.Spawn("eco.chinook", 0, new(6000, 1800)); m.Step();
        Need(dock.LoadingId == a.Id && a.Activity == EntityActivity.Loading && b.Activity == EntityActivity.Waiting, "Only one loader owns dock");
        Steps(m, config.Rules.GatherLoadTicks - 1);
        Need(a.Cargo == config.Role("eco.chinook").CargoCapacity && b.Cargo == 0, "Load completion is exclusive");
        m.Step(); Need(a.TargetId == ownDrop.Id, "Gatherer chooses own dropoff, not closer ally");
        Until(m, () => m.Snapshot(0).Player.Money == config.Rules.StartingCash + config.Role("eco.chinook").CargoCapacity * 2, 400, "Both finite loads delivered");
        Need(dock.Supplies == 0 && m.Snapshot(1).Player.Money == config.Rules.StartingCash, "No ally cash sharing or infinite supply");
        m.Hit(dock, int.MaxValue, a); Need(m.Bodies.ContainsKey(dock.Id), "Dock indestructible");
    }
    private static void Combat()
    {
        var m = New(); var tank = m.Spawn("armor.basic", 0, new(6000, 1800)); var air = m.Spawn("eco.chinook", 1, new(7000, 1800)); air.AutoGather = false; m.Step();
        Need(!m.Submit(new MatchOrder(0, OrderKind.Attack, new[] { tank.Id }, air.Id)).Accepted, "Cannon cannot target air"); Steps(m, 100); Need(air.Hp == m.MaxHp(air), "Cannon auto attack does not hit air");
        var rocket = m.Spawn("inf.rocket", 0, new(6000, 2200)); Send(m, OrderKind.Attack, rocket, air.Id); m.Step();
        Need(m.Snapshot(0).Projectiles.Length > 0 && air.Hp == m.MaxHp(air), "Missile occupies world before impact");
        Until(m, () => !m.Bodies.ContainsKey(air.Id), 600, "Rocket must kill air");
        m = New(); var rifle = m.Spawn("inf.rifle", 0, new(6000, 2000)); rocket = m.Spawn("inf.rocket", 1, new(6900, 2000));
        Until(m, () => !m.Bodies.ContainsKey(rifle.Id) || !m.Bodies.ContainsKey(rocket.Id), 300, "Rifle rocket duel completes");
        Need(m.Bodies.ContainsKey(rifle.Id) && !m.Bodies.ContainsKey(rocket.Id), "Rocket poor vs infantry loses equal duel");
        m = New(); tank = m.Spawn("armor.basic", 0, new(6000, 2000));
        var rockets = Enumerable.Range(0, 3).Select(i => m.Spawn("inf.rocket", 1, new(6900, 1900 + i * 150))).ToArray();
        Until(m, () => !m.Bodies.ContainsKey(tank.Id) || rockets.All(r => !m.Bodies.ContainsKey(r.Id)), 700, "Comparable-cost rocket tank fight completes");
        Need(!m.Bodies.ContainsKey(tank.Id), "Rocket composition counters tank");
    }
    private static void FriendlyFire()
    {
        var m = New(allied: true); var tank = m.Spawn("armor.basic", 0, new(6000, 2000)); var infantry = m.Spawn("inf.rifle", 1, new(6150, 2000)); m.Step(); Need(infantry.Hp == m.MaxHp(infantry), "Automatic fire avoids allies");
        Send(m, OrderKind.Move, tank, pos: new(6600, 2000)); m.Step(); Need(!m.Bodies.ContainsKey(infantry.Id), "Tank crush hits allied infantry");
        var friend = m.Spawn("build.dozer", 0, new(7000, 2000)); Send(m, OrderKind.ForceAttack, tank, friend.Id); Steps(m, config.Role("armor.basic").AttackCooldownTicks + 2); Need(friend.Hp < m.MaxHp(friend), "Force attack damages own units");
        var rocket = m.Spawn("inf.rocket", 0, new(7000, 3000));
        Send(m, OrderKind.ForceAttack, rocket, pos: friend.Pos); m.Step();
        Need(m.Snapshot(0).Projectiles.Any(p => p.SourceId == rocket.Id && p.TargetId == 0), "Ground force attack launches a real missile");
        var hp = friend.Hp; Steps(m, 15); Need(friend.Hp < hp, "Ground missile impact damages friendlies in its configured radius");
    }
    private static void Garrison()
    {
        var m = New(); var box = m.Spawn("map.garrison", -1, new(6000, 2000)); var rifle = m.Spawn("inf.rifle", 0, new(5700, 2000)); m.Step();
        Send(m, OrderKind.Enter, rifle, box.Id); m.Step(); Need(rifle.ContainerId == box.Id && box.Owner == 0, "Entry occupies neutral garrison");
        var enemy = m.Spawn("inf.rifle", 1, new(6900, 2000)); m.Step();
        Need(box.Hp == m.MaxHp(box) - config.Role("inf.rifle").Damage * config.Damage.Single(d => d.Id == "dmg.small").ArmorPercent["arm.garrison"] / 100, "Occupied garrison applies poor small-arms armor");
        Need(enemy.Hp < m.MaxHp(enemy), "Passenger fires from garrison");
        m.Hit(box, box.Hp, null); Need(m.Bodies.ContainsKey(rifle.Id) && rifle.ContainerId == 0 && rifle.Hp <= m.MaxHp(rifle) * config.Rules.GarrisonSpillHpPercent / 100, "Garrison destruction spills hurt survivors");
    }
    private static void Transport()
    {
        var m = New(); var scout = m.Spawn("veh.scout_gun", 0, new(6000, 2000)); var rocket = m.Spawn("inf.rocket", 0, new(5700, 2000)); m.Step();
        Send(m, OrderKind.Enter, rocket, scout.Id); m.Step(); var enemy = m.Spawn("armor.basic", 1, new(6900, 2000)); m.Step();
        Need(rocket.ContainerId == scout.Id && rocket.Cooldown > 0 && m.Snapshot(0).Projectiles.Any(p => p.SourceId == rocket.Id), "Scout passengers fire independent weapons");
        Send(m, OrderKind.Exit, rocket, pos: new(11000, 7000)); m.Step(); Need(rocket.ContainerId == 0 && Distance2(rocket.Pos, scout.Pos) < 1000L * 1000, "Exit cannot teleport across map");
        m = New(); var air = m.Spawn("eco.chinook", 0, new(6000, 2000)); rocket = m.Spawn("inf.rocket", 0, new(5700, 2000)); m.Step();
        Send(m, OrderKind.Enter, rocket, air.Id); m.Step(); m.Spawn("armor.basic", 1, new(6900, 2000)); Steps(m, 40);
        Need(rocket.ContainerId == air.Id && rocket.Cooldown == 0 && m.Snapshot(0).Projectiles.Length == 0, "Gatherer passengers do not fire out");
        Send(m, OrderKind.Move, air, pos: new(6000, 3500)); Steps(m, 10); Need(rocket.Pos == air.Pos, "Transport carries passengers");
        m.Destroy(air, null); Need(!m.Bodies.ContainsKey(rocket.Id), "Destroyed flying transport loses occupants");
    }
    private static void Capture()
    {
        var m = New(); var barracks = m.Spawn("prod.barracks", 0, new(5000, 1000)); m.Spawn("power.fusion", 0, new(4000, 1000)); var rifle = m.Spawn("inf.rifle", 0, new(6000, 2000)); var target = m.Spawn("prod.command", 1, new(6600, 2000)); m.Step();
        Need(!m.Submit(new MatchOrder(0, OrderKind.Capture, new[] { rifle.Id }, target.Id)).Accepted, "Capture gated by research");
        Send(m, OrderKind.Queue, barracks, product: "up.capture"); Steps(m, config.Role("up.capture").BuildTicks);
        Need(m.Snapshot(0).Player.Upgrades.Contains("up.capture"), "Research finishes via paid queue");
        Send(m, OrderKind.Capture, rifle, target.Id); m.Step(); Need(target.Owner == 1 && rifle.CaptureLeft > 0, "Capture is a channel");
        m.Hit(rifle, 1, null); Need(rifle.CaptureLeft == 0 && rifle.Actions.Count == 0, "Damage interrupts capture");
        for (int i = 0; i < 3; i++) Send(m, OrderKind.Queue, target, product: "build.dozer");
        m.Step(); var enemyMoney = m.Snapshot(1).Player.Money;
        Send(m, OrderKind.Capture, rifle, target.Id); Steps(m, config.Rules.CaptureTicks);
        Need(target.Owner == 0 && target.Queue.Count == 0, "Capture transfers and clears original production");
        Need(m.Snapshot(1).Player.Money > enemyMoney, "Cancelled capture queues refund original owner");
    }
    private static void Repair()
    {
        var m = New(allied: true); var builder = m.Spawn("build.dozer", 0, new(6000, 2000)); var building = m.Spawn("power.fusion", 1, new(6500, 2000)); building.Hp -= config.Role("armor.basic").Damage; m.Step();
        int beforeHp = building.Hp, beforeCash = m.Snapshot(0).Player.Money;
        Send(m, OrderKind.Repair, builder, building.Id); m.Step(); Need(building.Hp > beforeHp && m.Snapshot(0).Player.Money < beforeCash && m.Snapshot(1).Player.Money == config.Rules.StartingCash, "Builder owner pays allied repairs");
        var factory = m.Spawn("prod.factory", 0, new(6000, 4000)); m.Spawn("power.fusion", 0, new(4800, 4000)); var ownVehicle = m.Spawn("build.dozer", 0, new(6500, 4000)); var allyVehicle = m.Spawn("build.dozer", 1, new(6500, 4200)); ownVehicle.Hp -= 100; allyVehicle.Hp -= 100; int ownHp = ownVehicle.Hp, allyHp = allyVehicle.Hp; m.Step();
        Need(ownVehicle.Hp > ownHp && allyVehicle.Hp == allyHp, "Factory repairs only owner's vehicles");
    }
    private static void Veterancy()
    {
        var m = New(); var unit = m.Spawn("armor.basic", 0, new(6000, 2000)); int baseHp = unit.Hp;
        while (unit.Experience < config.Ranks[3].Experience) { var victim = m.Spawn("armor.basic", 1, new(7000, 1000)); m.Hit(victim, victim.Hp, unit); }
        Need(unit.Rank == 3 && unit.Hp == m.MaxHp(unit) && unit.Hp > baseHp, "Kills grant Heroic health bonus");
        m.Hit(unit, config.Role("armor.basic").Damage, null); int hurt = unit.Hp; Steps(m, config.Rules.SelfHealDelayTicks - 1); Need(unit.Hp == hurt, "Self-heal waits after damage"); m.Step(); Need(unit.Hp > hurt, "Heroic self-heal starts after configured delay");
        var friend = m.Spawn("armor.basic", 0, new(8000, 1000)); int xp = unit.Experience; m.Hit(friend, friend.Hp, unit); Need(unit.Experience == xp, "Friendly kills grant no XP");
    }
    private static void Intel()
    {
        var m = New(fog: true); var enemy = m.Spawn("prod.barracks", 1, new(3200, 4000)); enemy.Queue.Add(new Match.Production { RoleId = "inf.rifle", Paid = 150, Left = 999, Total = 999 }); enemy.Rally = new(5000, 3000); enemy.Cargo = 100; m.Step();
        var seen = m.Snapshot(0).Entities.Single(e => e.Id == enemy.Id); Need(seen.Queue.Length == 0 && seen.RallyPoint == default && seen.Cargo == 0 && seen.TargetId == 0, "Enemy private values redacted");
        Own(m, "prod.command").Pos = new(800, 800); Own(m, "build.dozer").Pos = new(800, 1300); m.Step(); int rememberedHp = m.Snapshot(0).Entities.Single(e => e.Id == enemy.Id).Hp;
        enemy.Hp -= 100; m.Step(); var stale = m.Snapshot(0).Entities.Single(e => e.Id == enemy.Id); Need(stale.IsRemembered && stale.Hp == rememberedHp, "Unseen building state remains stale");
        var hidden = m.Spawn("inf.rifle", 1, new(6000, 6000)); m.Step(); Need(!m.Snapshot(0).Entities.Any(e => e.Id == hidden.Id), "Hidden units absent");
        m.Destroy(enemy, null); m.Step(); Need(m.Snapshot(0).Entities.Any(e => e.Id == enemy.Id && e.IsRemembered), "Hidden destruction does not leak");
        Own(m, "build.dozer").Pos = new(3000, 4000); m.Step(); Need(!m.Snapshot(0).Entities.Any(e => e.Id == enemy.Id), "LOS clears destroyed memory");
        var hash = m.StateHash(); var snapshot = m.Snapshot(0); snapshot.Visibility[0] = Visibility.Visible; snapshot.Entities[0] = new EntitySnapshot(); var copy = m.Config; copy.Roles[0] = new RoleConfig(); var setup = m.Setup; setup.Slots[0] = new SlotSetup(); Need(m.StateHash() == hash, "Output mutation cannot change simulation");
        var blocker = m.Spawn("power.fusion", 1, new(6000, 4500)); var builder = Own(m, "build.dozer");
        Need(m.CanPlace(0, builder.Id, "power.fusion", blocker.Pos).Allowed, "Preview cannot reveal hidden footprint");
        int cash = m.Snapshot(0).Player.Money; Send(m, OrderKind.Build, builder, product: "power.fusion", pos: blocker.Pos); Steps(m, 500);
        Need(m.Snapshot(0).Player.Money == cash && !m.Bodies.Values.Any(b => b.Owner == 0 && b.RoleId == "power.fusion"), "Full collision blocks construction without orphan or charge");
    }
    private static void Movement()
    {
        var c = config with { Map = config.Map with { Objects = Array.Empty<MapObjectConfig>() } };
        var s = c.CreateSetup() with { Fog = false, Slots = c.DefaultSlots.Select(p => p.Index == 1 ? p with { Occupant = Occupant.Player } : p).ToArray() };
        var m = (Match)new MatchFactory().Create(c, s); var ground = m.Spawn("build.dozer", 0, new(4200, 1400)); var air = m.Spawn("eco.chinook", 0, new(4200, 1400)); air.AutoGather = false;
        var destination = new WorldPoint(6100, 1400); Send(m, OrderKind.Move, ground, pos: destination); Send(m, OrderKind.Move, air, pos: destination); Steps(m, 60);
        Need(Distance2(air.Pos, destination) <= (long)config.Role("eco.chinook").Radius * config.Role("eco.chinook").Radius && ground.Pos != air.Pos, "Air crosses obstacle while ground routes around");
        Until(m, () => ground.Actions.Count == 0, 600, "Ground path finds route around blocked rectangle");
        Need(Distance2(ground.Pos, destination) <= (long)config.Role("build.dozer").Radius * config.Role("build.dozer").Radius, "Ground reaches legal target");
        Send(m, OrderKind.Move, ground, pos: new(6000, 3500)); Send(m, OrderKind.Waypoint, ground, pos: new(6500, 4000));
        Until(m, () => ground.Actions.Count == 0 && m.Tick > 61, 500, "Waypoints complete");
        // Submit is deferred, so execute pending waypoints before checking the resulting queue.
        m.Step(); Until(m, () => ground.Actions.Count == 0, 500, "Appended waypoint reaches final target"); Need(Distance2(ground.Pos, new(6500, 4000)) <= 100L * 100, "Waypoint appended rather than replaced");
        var attacker = m.Spawn("armor.basic", 0, new(6000, 4500));
        Send(m, OrderKind.AttackMove, attacker, pos: c.Map.Starts.Single(s => s.Slot == 1).Command); m.Step();
        var previous = attacker.Pos; Steps(m, 20); Need(attacker.Pos != previous, "Attack-move approaches an occupied enemy building footprint");
        var escort = m.Spawn("inf.rifle", 0, new(6500, 4400)); Send(m, OrderKind.Guard, escort, ground.Id); Steps(m, 20); Need(escort.Actions.Count == 1 && escort.Actions[0].Kind == OrderKind.Guard, "Guard persists");
    }
    private static void Outcome()
    {
        var c = config with { Map = config.Map with { MaxPlayers = 3, Starts = config.Map.Starts.Concat(new[] { new StartConfig(7, new(6000, 7000), new(6500, 7000)) }).ToArray() } };
        var s = c.CreateSetup() with { Fog = false, Slots = c.DefaultSlots.Select(p => p.Index == 1 ? p with { Occupant = Occupant.Player, Team = 0 } : p.Index == 7 ? p with { Occupant = Occupant.Player, Team = 0 } : p).ToArray() };
        var m = (Match)new MatchFactory().Create(c, s);
        Need(m.Snapshot(0).Entities.Count(e => e.RoleId == "map.dock") == c.Map.Objects.Count(e => e.RoleId == "map.dock"), "All neutral docks spawn");
        m.Destroy(Own(m, "build.dozer", 1), null); m.Step(); Need(!m.Snapshot(0).EliminatedSlots.Contains(1), "Army wipe is recoverable");
        var incomplete = m.Spawn("power.fusion", 1, new(11000, 1000), false); m.Destroy(Own(m, "prod.command", 1), null); m.Step(); Need(!m.Snapshot(0).EliminatedSlots.Contains(1), "Incomplete building prevents elimination");
        m.Destroy(incomplete, null); m.Step(); Need(m.Snapshot(0).EliminatedSlots.Contains(1) && m.Snapshot(0).Phase == MatchPhase.Running, "Third hostile slot keeps match running");
        m.Destroy(Own(m, "prod.command", 7), null); m.Step(); Need(m.Snapshot(0).WinningSlots.SequenceEqual(new[] { 0 }), "Eliminate all enemy teams to win");
    }
    private static void Ai()
    {
        var c = config with { Map = config.Map with { Objects = Array.Empty<MapObjectConfig>(), Terrain = Array.Empty<TerrainRect>() } };
        var first = (Match)new MatchFactory().Create(c, c.CreateSetup());
        var second = (Match)new MatchFactory().Create(c, c.CreateSetup());
        first.Spawn("inf.rocket", 0, new(800, 1200)); second.Spawn("inf.rocket", 0, new(4000, 1200));
        Steps(first, 2); Steps(second, 2);
        Need(JsonSerializer.Serialize(first.Snapshot(1)) == JsonSerializer.Serialize(second.Snapshot(1)), "AI decisions do not depend on hidden enemy army locations");
        var m = (Match)new MatchFactory().Create(c, c.CreateSetup());
        var defender = m.Spawn("armor.basic", 1, new(6500, 4000));
        var intruder = m.Spawn("inf.rifle", 0, new(9300, 4000));
        Steps(m, 2);
        Need(defender.Actions.Count > 0 && defender.Actions[0].Kind == OrderKind.AttackMove && defender.Actions[0].Position == intruder.Pos, "AI pulls its army back toward visible base threats");
        m = (Match)new MatchFactory().Create(c, c.CreateSetup());
        var placements = new[] { ("power.fusion", new WorldPoint(11100, 3000)), ("eco.dropoff", new WorldPoint(10200, 5000)), ("prod.barracks", new WorldPoint(9200, 2000)), ("prod.factory", new WorldPoint(10500, 1800)), ("def.patriot", new WorldPoint(11000, 4000)), ("power.fusion", new WorldPoint(11000, 6000)) };
        foreach (var placement in placements) m.Spawn(placement.Item1, 1, placement.Item2);
        m.Spawn("map.dock", -1, new(8900, 5400)).Supplies = 0;
        var fresh = m.Spawn("map.dock", -1, new(6000, 4000)); fresh.Supplies = config.Role("eco.chinook").CargoCapacity * 3;
        m.Spawn("veh.scout_gun", 1, new(6000, 2500));
        m.Spawn("eco.chinook", 1, new(8900, 5400)); // Own sight learns the empty home dock.
        Steps(m, 2);
        var builder = Own(m, "build.dozer", 1);
        Need(builder.Actions.Count > 0 && builder.Actions[0].ProductId == "eco.dropoff" && Distance2(builder.Actions[0].Position, fresh.Pos) <= (long)config.Ai.BuildSpacing * config.Ai.BuildSpacing * 2, "AI orders a paid expansion near revealed remaining supply");
        Until(m, () => m.Bodies.Values.Count(b => b.Owner == 1 && b.RoleId == "eco.dropoff" && b.Complete) == 2, 500, "AI expansion completes using its real dozer");
    }
    private static long Distance2(WorldPoint a, WorldPoint b) => (long)(a.X - b.X) * (a.X - b.X) + (long)(a.Z - b.Z) * (a.Z - b.Z);
}
