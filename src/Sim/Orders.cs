using System;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    public OrderReceipt Submit(MatchOrder order)
    {
        if (order == null || order.ActorIds == null) return new(false, "Invalid order.");
        var copy = order with { ActorIds = (int[])order.ActorIds.Clone() };
        var reason = Validate(copy);
        if (reason != "") return new(false, reason);
        pending.Add(copy);
        return new(true, "");
    }
    private string Validate(MatchOrder order)
    {
        if (phase != MatchPhase.Running) return "Match has ended.";
        if (!Players.TryGetValue(order.Slot, out var player) || player.Eliminated) return "Inactive player.";
        if (!Enum.IsDefined(order.Kind)) return "Unknown order.";
        if (order.Kind == OrderKind.Resign) return order.ActorIds.Length == 0 ? "" : "Resign takes no units.";
        if (order.ActorIds.Length == 0 || order.ActorIds.Distinct().Count() != order.ActorIds.Length) return "Select owned units.";
        foreach (var id in order.ActorIds) if (!Bodies.TryGetValue(id, out var b) || b.Owner != order.Slot) return "Select owned units.";
        var actors = order.ActorIds.Select(id => Bodies[id]).ToArray();
        if (actors.Any(b => b.ContainerId != 0) && order.Kind != OrderKind.Exit) return "Exit the container first.";
        var target = Bodies.GetValueOrDefault(order.TargetId);
        bool known = target != null && (target.Owner == order.Slot || Visible(order.Slot, target.Pos));
        if (order.Kind is OrderKind.Move or OrderKind.AttackMove or OrderKind.Waypoint or OrderKind.Build or OrderKind.Rally or OrderKind.Exit || order.Kind == OrderKind.ForceAttack && order.TargetId == 0 || order.Kind == OrderKind.Guard && order.TargetId == 0)
            if (!InBounds(order.Position)) return "Outside map.";
        if (actors.Any(b => !b.Complete) && order.Kind != OrderKind.Sell) return "Building is incomplete.";
        switch (order.Kind)
        {
            case OrderKind.Build:
                if (actors.Length != 1) return "Choose one dozer.";
                var placement = CanPlace(order.Slot, actors[0].Id, order.ProductId, order.Position);
                return placement.Allowed ? "" : placement.Reason;
            case OrderKind.Queue:
                if (actors.Length != 1 || !roles.TryGetValue(order.ProductId, out var product)) return "Choose a product.";
                if (product.ProducerId != actors[0].RoleId || product.ProducerId == "") return "Wrong producer.";
                if (!HasPrerequisites(order.Slot, product)) return "Missing prerequisite.";
                if (actors[0].Queue.Count >= C.Rules.QueueLimit) return "Queue full.";
                if (order.ProductId == "up.capture" && (player.Upgrades.Contains(order.ProductId) || Bodies.Values.Any(b => b.Owner == order.Slot && b.Queue.Any(q => q.RoleId == order.ProductId)))) return "Already researched or queued.";
                return player.Money >= product.Cost ? "" : "Insufficient funds.";
            case OrderKind.CancelQueue:
                return actors.Length == 1 && order.QueueIndex >= 0 && order.QueueIndex < actors[0].Queue.Count ? "" : "Queue item unavailable.";
            case OrderKind.Sell:
                return actors.All(b => Role(b).IsBuilding && !Role(b).IsNeutral) ? "" : "Select owned buildings.";
            case OrderKind.Rally:
                return actors.All(b => roles.Values.Any(r => r.ProducerId == b.RoleId)) ? "" : "Select production buildings.";
            case OrderKind.Attack:
            case OrderKind.ForceAttack:
                if (actors.Any(b => Role(b).Damage <= 0)) return "Select combat units.";
                if (order.Kind == OrderKind.ForceAttack && order.TargetId == 0) return "";
                if (!known || target!.ContainerId != 0 || Role(target).Indestructible) return "Target unavailable.";
                if (order.Kind == OrderKind.Attack && !Enemy(order.Slot, target.Owner)) return "Choose a hostile target.";
                return actors.All(b => DamagePercent(Role(b).DamageId, target) > 0) ? "" : "Weapon cannot hit this target.";
            case OrderKind.Repair:
                return actors.All(b => b.RoleId == "build.dozer") && known && Allied(order.Slot, target!.Owner) && Role(target).IsBuilding && target.Complete ? "" : "Choose an owned or allied building to repair.";
            case OrderKind.Capture:
                if (!player.Upgrades.Contains("up.capture") || actors.Any(b => b.RoleId != "inf.rifle")) return "Capture research required.";
                return known && Enemy(order.Slot, target!.Owner) && Role(target).IsBuilding && !Role(target).IsNeutral && target.Complete ? "" : "Choose a visible enemy building.";
            case OrderKind.Gather:
                return actors.All(b => b.RoleId == "eco.chinook" && b.Occupants.Count == 0) && known && target!.RoleId == "map.dock" && target.Supplies > 0 ? "" : "Choose an available supply dock.";
            case OrderKind.Enter:
                if (!known || !target!.Complete || Role(target).Capacity <= 0 || target.Owner != order.Slot && !(target.Owner == -1 && target.RoleId == "map.garrison")) return "Choose your transport or an empty garrison.";
                if (target.RoleId == "eco.chinook" && target.Cargo > 0) return "Unload supplies first.";
                if (actors.Any(b => !Role(b).IsInfantry)) return "Only infantry may enter.";
                return target.Occupants.Count + actors.Length <= Role(target).Capacity ? "" : "Container full.";
            case OrderKind.Exit:
                return actors.All(b => b.ContainerId != 0 || b.Occupants.Count > 0) ? "" : "No occupants to exit.";
            case OrderKind.Guard:
                if (order.TargetId != 0 && (!known || !Allied(order.Slot, target!.Owner))) return "Choose an owned or allied guard target.";
                goto case OrderKind.Move;
            case OrderKind.AttackMove:
                if (actors.Any(b => Role(b).Damage <= 0)) return "Select combat units.";
                goto case OrderKind.Move;
            case OrderKind.Stop:
            case OrderKind.Waypoint:
            case OrderKind.Move:
                return actors.All(b => Role(b).SpeedPerTick > 0) ? "" : "Select mobile units.";
            default: return "Unsupported order.";
        }
    }
    private bool HasPrerequisites(int slot, RoleConfig role) => role.Prerequisites.All(id => Bodies.Values.Any(b => b.Owner == slot && b.RoleId == id && b.Complete));
    public PlacementResult CanPlace(int slot, int builderId, string roleId, WorldPoint position)
    {
        if (!Players.TryGetValue(slot, out var p) || p.Eliminated || !Bodies.TryGetValue(builderId, out var builder) || builder.Owner != slot || builder.RoleId != "build.dozer") return new(false, "Choose your dozer.");
        if (!roles.TryGetValue(roleId, out var role) || !role.IsBuilding || role.IsNeutral) return new(false, "Not a buildable role.");
        if (!HasPrerequisites(slot, role)) return new(false, "Missing prerequisite.");
        if (p.Money < role.Cost) return new(false, "Insufficient funds.");
        if (!TerrainFits(position, role.Radius)) return new(false, "Blocked terrain.");
        if (Bodies.Values.Any(b => Obstacle(b) && (b.Owner == slot || Visible(slot, b.Pos)) && Near(position, b.Pos, role.Radius + Role(b).Radius))) return new(false, "Building footprint blocked.");
        if (p.Memory.Values.Any(b => b.IsRemembered && (roles[b.RoleId].IsBuilding || b.RoleId == "map.dock") && Near(position, b.Position, role.Radius + roles[b.RoleId].Radius))) return new(false, "Known footprint blocked.");
        return new(true, "");
    }
    private void Execute(MatchOrder order)
    {
        var player = Players[order.Slot];
        if (order.Kind == OrderKind.Resign)
        {
            player.Eliminated = true; Event("vo.defeat", order.Slot);
            foreach (var body in Bodies.Values.Where(b => b.Owner == order.Slot).ToArray()) Destroy(body, null);
            return;
        }
        foreach (var id in order.ActorIds)
        {
            if (!Bodies.TryGetValue(id, out var actor)) continue;
            switch (order.Kind)
            {
                case OrderKind.Queue:
                    var r = roles[order.ProductId]; player.Money -= r.Cost;
                    actor.Queue.Add(new Production { RoleId = r.Id, Paid = r.Cost, Left = r.BuildTicks, Total = r.BuildTicks }); break;
                case OrderKind.CancelQueue:
                    player.Money += actor.Queue[order.QueueIndex].Paid * C.Rules.QueueRefundPercent / 100;
                    actor.Queue.RemoveAt(order.QueueIndex); break;
                case OrderKind.Sell:
                    player.Money += roles[actor.RoleId].Cost * (actor.Complete ? C.Rules.SellRefundPercent : C.Rules.ConstructionCancelRefundPercent) / 100;
                    RefundQueue(actor); Destroy(actor, null); break;
                case OrderKind.Rally: actor.Rally = order.Position; break;
                case OrderKind.Exit: Exit(actor, order.Position); break;
                default:
                    if (!order.Append && order.Kind != OrderKind.Waypoint) Abort(actor);
                    if (order.Kind != OrderKind.Stop) actor.Actions.Add(order with { ActorIds = new[] { id }, Append = false, Position = order.TargetId != 0 && Bodies.TryGetValue(order.TargetId, out var target) ? target.Pos : order.Position });
                    break;
            }
        }
    }
    private void RefundQueue(Body body)
    {
        if (Players.TryGetValue(body.Owner, out var player)) foreach (var q in body.Queue) player.Money += q.Paid * C.Rules.QueueRefundPercent / 100;
        body.Queue.Clear();
    }
    private void Abort(Body body)
    {
        if (body.ConstructionId != 0 && Bodies.TryGetValue(body.ConstructionId, out var site))
        {
            Players[body.Owner].Money += Role(site).Cost * C.Rules.ConstructionCancelRefundPercent / 100;
            Destroy(site, null);
        }
        ReleaseDock(body); body.Timer = 0; body.CaptureLeft = 0; body.ConstructionId = 0;
        body.AutoGather = false; body.Actions.Clear(); body.Path.Clear(); body.TargetId = 0; body.Activity = EntityActivity.Idle; body.Destination = body.Pos;
    }
    private void FinishAction(Body body)
    {
        if (body.Actions.Count > 0) body.Actions.RemoveAt(0);
        body.Path.Clear(); body.TargetId = 0; body.CaptureLeft = 0; body.Timer = 0; body.Activity = EntityActivity.Idle;
    }
}
