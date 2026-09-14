using System;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    private void StepBody(Body body)
    {
        if (!body.Complete || body.Owner >= 0 && Players[body.Owner].Eliminated) return;
        if (body.Actions.Count > 0 && body.Actions[0].Slot != body.Owner) Abort(body);
        if (body.Cooldown > 0) body.Cooldown--;
        if (Tick - body.LastHit >= C.Rules.SelfHealDelayTicks && C.Ranks[body.Rank].HealHpPerTick > 0) body.Hp = Math.Min(MaxHp(body), body.Hp + C.Ranks[body.Rank].HealHpPerTick);
        if (body.ContainerId != 0)
        {
            body.Activity = EntityActivity.Contained;
            if (Bodies.TryGetValue(body.ContainerId, out var container) && Role(container).FireFromTransport) AutoFire(body, container.Pos);
            return;
        }
        if (body.Actions.Count > 0 && body.Actions[0].Kind == OrderKind.Exit)
        {
            QueuedExit(body, body.Actions[0]);
            return;
        }
        if (Role(body).IsBuilding)
        {
            StepProduction(body);
            if (body.Powered)
            {
                if (body.Actions.Count > 0 && body.Actions[0].Kind is OrderKind.Attack or OrderKind.ForceAttack) AttackAction(body, body.Actions[0]);
                else AutoFire(body, body.Pos);
            }
            if (body.RoleId == "prod.factory" && body.Powered)
                foreach (var vehicle in Bodies.Values.Where(b => b.Owner == body.Owner && b.ContainerId == 0 && !Role(b).IsBuilding && !Role(b).IsFlying && !Role(b).IsInfantry && Near(body.Pos, b.Pos, C.Rules.FactoryRepairRange + Role(body).Radius)))
                    vehicle.Hp = Math.Min(MaxHp(vehicle), vehicle.Hp + C.Rules.FactoryRepairHpPerTick);
            return;
        }
        if (body.Actions.Count == 0)
        {
            body.Activity = EntityActivity.Idle;
            if (body.RoleId == "eco.chinook" && body.Occupants.Count == 0 && (body.AutoGather || body.Cargo > 0 && InDeliveryRange(body))) Gather(body);
            else AutoFire(body, body.Pos);
            return;
        }
        var action = body.Actions[0];
        switch (action.Kind)
        {
            case OrderKind.Move:
            case OrderKind.Waypoint:
                body.Activity = EntityActivity.Moving;
                if (MoveToward(body, action.Position, 0) != TravelResult.Moving) FinishAction(body);
                break;
            case OrderKind.Attack:
            case OrderKind.ForceAttack: AttackAction(body, action); break;
            case OrderKind.AttackMove:
            case OrderKind.Guard:
                var guardPos = action.TargetId != 0 && Bodies.TryGetValue(action.TargetId, out var guarded) && (guarded.Owner == body.Owner || Visible(body.Owner, guarded.Pos)) ? guarded.Pos : action.Position;
                var enemy = Acquire(body, Role(body).SightRange);
                if (enemy != null && (action.Kind != OrderKind.Guard || Near(guardPos, enemy.Pos, C.Rules.GuardLeash))) Engage(body, enemy);
                else
                {
                    body.Activity = EntityActivity.Moving;
                    if (MoveToward(body, guardPos, action.Kind == OrderKind.Guard ? C.Rules.InteractionRange : Role(body).Radius) != TravelResult.Moving && action.Kind != OrderKind.Guard) FinishAction(body);
                }
                break;
            case OrderKind.Build: Build(body, action); break;
            case OrderKind.Repair: Repair(body, action); break;
            case OrderKind.Capture: Capture(body, action); break;
            case OrderKind.Gather:
                body.GatherDockId = action.TargetId; Gather(body); break;
            case OrderKind.Enter: Enter(body, action); break;
        }
    }
    private void UpdatePower()
    {
        foreach (var p in Players.Values)
        {
            var own = Bodies.Values.Where(b => b.Owner == p.Slot && b.Complete).ToArray();
            p.Supply = own.Sum(b => Role(b).PowerSupply); p.Demand = own.Sum(b => Role(b).PowerDrain);
            var low = p.Demand > p.Supply;
            if (low && !p.LowPower) Event("vo.power", p.Slot);
            p.LowPower = low; p.Radar = !low && own.Any(b => b.RoleId == "prod.command");
            foreach (var b in Bodies.Values.Where(b => b.Owner == p.Slot)) b.Powered = !low || Role(b).PowerDrain == 0;
        }
    }
    private void Build(Body builder, MatchOrder action)
    {
        var role = roles[action.ProductId];
        builder.Destination = action.Position; builder.Activity = EntityActivity.Building;
        if (builder.ConstructionId == 0)
        {
            if (MoveToward(builder, action.Position, role.Radius + Role(builder).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) return;
            if (!ClearConstructionFootprint(builder, action.Position, role.Radius)) return;
            var existing = Bodies.Values.FirstOrDefault(b => !b.Complete && b.Owner == builder.Owner && b.RoleId == role.Id && Near(b.Pos, action.Position, role.Radius));
            Body site;
            if (existing != null)
            {
                site = existing; site.BuilderId = builder.Id; site.Facing = action.Facing != 0 ? action.Facing : site.Facing;
            }
            else
            {
                if (!HasPrerequisites(builder.Owner, role) || Players[builder.Owner].Money < role.Cost || !BuildFits(action.Position, role.Radius) || !GroundFits(action.Position, role.Radius, units: true) || Bodies.Count >= C.Rules.HardEntityCap)
                { if (Players[builder.Owner].Money < role.Cost) Event("vo.funds", builder.Owner, builder); FinishAction(builder); return; }
                Players[builder.Owner].Money -= role.Cost;
                site = Spawn(role.Id, builder.Owner, action.Position, false); site.Hp = 1; site.BuilderId = builder.Id; site.Facing = action.Facing;
            }
            builder.ConstructionId = site.Id; builder.TargetId = site.Id; builder.Path.Clear();
            return;
        }
        if (!Bodies.TryGetValue(builder.ConstructionId, out var building)) { builder.ConstructionId = 0; FinishAction(builder); return; }
        if (!Near(builder.Pos, building.Pos, role.Radius + Role(builder).Radius + C.Rules.InteractionRange)) { MoveToward(builder, building.Pos, role.Radius + Role(builder).Radius + C.Rules.InteractionRange); return; }
        int before = role.Hp * (building.BuildTotal - building.BuildLeft) / Math.Max(1, building.BuildTotal);
        building.BuildLeft = Math.Max(0, building.BuildLeft - 1);
        int after = role.Hp * (building.BuildTotal - building.BuildLeft) / Math.Max(1, building.BuildTotal);
        building.Hp = Math.Min(MaxHp(building), building.Hp + after - before);
        if (building.BuildLeft != 0) return;
        building.Complete = true; building.BuilderId = 0; builder.ConstructionId = 0;
        building.Rally = FrontOf(building);
        Event("vo.building_done", building.Owner, building);
        if (building.RoleId == "eco.dropoff" && !building.FreeGathererGranted)
        {
            building.FreeGathererGranted = SpawnProduced(building, roles["eco.chinook"]);
        }
        FinishAction(builder);
    }
    private void StepProduction(Body producer)
    {
        if (producer.RoleId == "eco.dropoff" && !producer.FreeGathererGranted)
            producer.FreeGathererGranted = SpawnProduced(producer, roles["eco.chinook"]);
        if (producer.Queue.Count == 0 || !producer.Powered) return;
        var q = producer.Queue[0];
        if (q.Left > 0) q.Left--;
        if (q.Left > 0) return;
        if (q.RoleId == "up.capture") { Players[producer.Owner].Upgrades.Add(q.RoleId); Event("vo.upgrade_done", producer.Owner, producer); }
        else if (!SpawnProduced(producer, roles[q.RoleId])) return;
        producer.Queue.RemoveAt(0);
    }
    private bool SpawnProduced(Body producer, RoleConfig role)
    {
        if (Bodies.Count >= C.Rules.HardEntityCap) return false;
        var requested = new WorldPoint(producer.Pos.X, producer.Pos.Z + Role(producer).Radius + role.Radius + C.Rules.SpawnOffset);
        if (!InBounds(requested)) requested = new WorldPoint(producer.Pos.X, producer.Pos.Z - Role(producer).Radius - role.Radius - C.Rules.SpawnOffset);
        var point = FindFree(requested, role);
        if (point == null) return false;
        var unit = Spawn(role.Id, producer.Owner, point.Value);
        Event("vo.unit_ready", producer.Owner, unit);
        if (producer.Rally != producer.Pos) unit.Actions.Add(new MatchOrder(unit.Owner, OrderKind.Move, new[] { unit.Id }, Position: producer.Rally));
        return true;
    }
    private void ReleaseDock(Body gatherer)
    {
        foreach (var dock in Bodies.Values.Where(b => b.RoleId == "map.dock" && b.LoadingId == gatherer.Id)) dock.LoadingId = 0;
    }
    private Body? KnownDock(Body gatherer)
    {
        var p = Players[gatherer.Owner];
        return Bodies.Values.Where(b => b.RoleId == "map.dock" && (Visible(gatherer.Owner, b.Pos) ? b.Supplies > 0 : p.Memory.TryGetValue(b.Id, out var remembered) && remembered.SuppliesLeft > 0))
            .OrderBy(b => Distance2(gatherer.Pos, b.Pos)).ThenBy(b => b.Id).FirstOrDefault();
    }
    private bool InDeliveryRange(Body unit) => Bodies.Values.Any(b => b.Owner == unit.Owner && b.RoleId == "eco.dropoff" && b.Complete && Near(unit.Pos, b.Pos, Role(b).Radius + C.Rules.InteractionRange));
    private void Gather(Body unit)
    {
        if (unit.Occupants.Count != 0) return;
        if (unit.Cargo > 0)
        {
            ReleaseDock(unit);
            var dropoff = Bodies.Values.Where(b => b.Owner == unit.Owner && b.RoleId == "eco.dropoff" && b.Complete).OrderBy(b => Distance2(unit.Pos, b.Pos)).ThenBy(b => b.Id).FirstOrDefault();
            if (dropoff == null) { unit.Activity = EntityActivity.Waiting; return; }
            unit.TargetId = dropoff.Id; unit.Activity = EntityActivity.Returning;
            if (MoveToward(unit, dropoff.Pos, Role(dropoff).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) { unit.Timer = 0; return; }
            if (++unit.Timer < C.Rules.GatherUnloadTicks) return;
            Players[unit.Owner].Money += unit.Cargo; unit.Cargo = 0; unit.Timer = 0; unit.TargetId = 0;
            return;
        }
        var dock = Bodies.GetValueOrDefault(unit.GatherDockId);
        if (dock == null || Visible(unit.Owner, dock.Pos) && dock.Supplies <= 0) dock = KnownDock(unit);
        if (dock == null) { ReleaseDock(unit); unit.Activity = EntityActivity.Idle; if (unit.Actions.Count > 0 && unit.Actions[0].Kind == OrderKind.Gather) FinishAction(unit); return; }
        unit.GatherDockId = dock.Id;
        if (unit.Actions.Count > 0 && unit.Actions[0].Kind == OrderKind.Gather) unit.Actions[0] = unit.Actions[0] with { TargetId = dock.Id };
        unit.TargetId = dock.Id; unit.Activity = EntityActivity.Gathering;
        if (MoveToward(unit, dock.Pos, Role(dock).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) { unit.Timer = 0; return; }
        if (dock.Supplies <= 0) { ReleaseDock(unit); unit.GatherDockId = 0; unit.Timer = 0; return; }
        if (dock.LoadingId != 0 && dock.LoadingId != unit.Id) { unit.Activity = EntityActivity.Waiting; return; }
        dock.LoadingId = unit.Id; unit.Activity = EntityActivity.Loading;
        if (++unit.Timer < C.Rules.GatherLoadTicks) return;
        unit.Cargo = Math.Min(Role(unit).CargoCapacity, dock.Supplies); dock.Supplies -= unit.Cargo; unit.Timer = 0; dock.LoadingId = 0;
    }
    private void Repair(Body builder, MatchOrder action)
    {
        if (!Bodies.TryGetValue(action.TargetId, out var building) || !Allied(builder.Owner, building.Owner)) { FinishAction(builder); return; }
        if (!building.Complete)
        {
            if (builder.RoleId != "build.dozer") { FinishAction(builder); return; }
            builder.ConstructionId = building.Id; building.BuilderId = builder.Id;
            Build(builder, action with { Kind = OrderKind.Build, ProductId = building.RoleId, Position = building.Pos, Facing = building.Facing });
            return;
        }
        builder.TargetId = building.Id; builder.Activity = EntityActivity.Repairing;
        if (MoveToward(builder, building.Pos, Role(building).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) return;
        int hp = Math.Min(C.Rules.RepairHpPerTick, MaxHp(building) - building.Hp);
        if (C.Rules.RepairCostPerHp > 0) hp = Math.Min(hp, Players[builder.Owner].Money / C.Rules.RepairCostPerHp);
        building.Hp += hp; Players[builder.Owner].Money -= hp * C.Rules.RepairCostPerHp;
        if (building.Hp == MaxHp(building)) FinishAction(builder);
    }
    private void Capture(Body rifle, MatchOrder action)
    {
        if (!Bodies.TryGetValue(action.TargetId, out var building) || !Enemy(rifle.Owner, building.Owner) || !building.Complete || !Visible(rifle.Owner, building.Pos)) { FinishAction(rifle); return; }
        rifle.TargetId = building.Id; rifle.Activity = EntityActivity.Capturing;
        if (MoveToward(rifle, building.Pos, Role(building).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) { rifle.CaptureLeft = 0; return; }
        if (rifle.CaptureLeft == 0) rifle.CaptureLeft = C.Rules.CaptureTicks;
        if (--rifle.CaptureLeft > 0) return;
        RefundQueue(building);
        if (building.BuilderId != 0 && Bodies.TryGetValue(building.BuilderId, out var builder)) { builder.ConstructionId = 0; Abort(builder); }
        Abort(building);
        building.BuilderId = 0; building.Owner = rifle.Owner; building.Rally = FrontOf(building);
        FinishAction(rifle);
    }
    private void Enter(Body infantry, MatchOrder action)
    {
        if (!Bodies.TryGetValue(action.TargetId, out var container) || !container.Complete || container.Owner != infantry.Owner && !(container.Owner == -1 && container.RoleId == "map.garrison") || container.Occupants.Count >= Role(container).Capacity || container.RoleId == "eco.chinook" && container.Cargo > 0)
        { FinishAction(infantry); return; }
        infantry.TargetId = container.Id; infantry.Activity = EntityActivity.Moving;
        if (MoveToward(infantry, container.Pos, Role(container).Radius + C.Rules.InteractionRange) != TravelResult.Arrived) return;
        if (container.RoleId == "eco.chinook" || container.Owner != infantry.Owner) Abort(container);
        container.Owner = infantry.Owner; container.Occupants.Add(infantry.Id); infantry.ContainerId = container.Id; infantry.Pos = container.Pos;
        FinishAction(infantry); infantry.Activity = EntityActivity.Contained;
    }
    private void QueuedExit(Body carrier, MatchOrder action)
    {
        foreach (var id in action.ActorIds)
            if (carrier.Owner == action.Slot && Bodies.TryGetValue(id, out var actor) && actor.Owner == action.Slot && (actor.Id == carrier.Id || actor.ContainerId == carrier.Id))
                Exit(actor, action.Position);
        FinishAction(carrier);
    }
    private void Exit(Body actor, WorldPoint desired)
    {
        var ids = actor.ContainerId != 0 ? new[] { actor.Id } : actor.Occupants.ToArray();
        foreach (var id in ids)
        {
            if (!Bodies.TryGetValue(id, out var infantry) || !Bodies.TryGetValue(infantry.ContainerId, out var container)) continue;
            // Unload is local to its container; a map click cannot teleport passengers across the map.
            var anchor = Near(container.Pos, desired, C.Rules.InteractionRange + Role(container).Radius) ? desired : Toward(container.Pos, desired, C.Rules.InteractionRange + Role(container).Radius);
            int max = C.Rules.InteractionRange + Role(container).Radius + Role(infantry).Radius + C.Map.CellSize;
            var point = FindFree(anchor, Role(infantry), infantry.Id, max, container.Pos);
            if (point == null) continue;
            container.Occupants.Remove(id); infantry.ContainerId = 0; infantry.Pos = point.Value; infantry.Destination = point.Value; infantry.Activity = EntityActivity.Idle;
            if (container.RoleId == "map.garrison" && container.Occupants.Count == 0) { Abort(container); container.Owner = -1; }
        }
    }
}
