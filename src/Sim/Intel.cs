using System;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    private bool InBounds(WorldPoint p) => p.X >= 0 && p.Z >= 0 && p.X < C.Map.WidthCells * C.Map.CellSize && p.Z < C.Map.HeightCells * C.Map.CellSize;
    private bool Visible(int slot, WorldPoint p) => InBounds(p) && Players.TryGetValue(slot, out var player) && player.Fog[p.Z / C.Map.CellSize * C.Map.WidthCells + p.X / C.Map.CellSize] == Visibility.Visible;
    private void UpdateFog()
    {
        foreach (var player in Players.Values)
        {
            for (var i = 0; i < player.Fog.Length; i++) if (!S.Fog) player.Fog[i] = Visibility.Visible; else if (player.Fog[i] == Visibility.Visible) player.Fog[i] = Visibility.Fog;
            if (S.Fog) foreach (var body in Bodies.Values.Where(b => b.Owner == player.Slot && b.Complete && b.ContainerId == 0))
            {
                var range = Role(body).SightRange; var cell = C.Map.CellSize;
                for (var z = Math.Max(0, (body.Pos.Z - range) / cell); z <= Math.Min(C.Map.HeightCells - 1, (body.Pos.Z + range) / cell); z++)
                    for (var x = Math.Max(0, (body.Pos.X - range) / cell); x <= Math.Min(C.Map.WidthCells - 1, (body.Pos.X + range) / cell); x++)
                        if (Near(body.Pos, new WorldPoint(x * cell + cell / 2, z * cell + cell / 2), range)) player.Fog[z * C.Map.WidthCells + x] = Visibility.Visible;
            }
            foreach (var stale in player.Memory.Values.ToArray())
                if (Visible(player.Slot, stale.Position) && (!Bodies.TryGetValue(stale.Id, out var existing) || existing.Owner == player.Slot || existing.Pos != stale.Position)) player.Memory.Remove(stale.Id);
            foreach (var body in Bodies.Values)
                if (body.Owner != player.Slot && Obstacle(body) && Visible(player.Slot, body.Pos)) player.Memory[body.Id] = Describe(body, false) with { IsRemembered = true };
        }
    }
    private EntitySnapshot Describe(Body b, bool own)
    {
        var statuses = new System.Collections.Generic.List<string>();
        if (b.CaptureLeft > 0) statuses.Add("st.capturing");
        if (b.ContainerId != 0) statuses.Add("st.garrisoned");
        if (!b.Powered) statuses.Add("st.low_power");
        return new EntitySnapshot
        {
            Id = b.Id, RoleId = b.RoleId, OwnerSlot = b.Owner, Position = b.ContainerId != 0 && Bodies.TryGetValue(b.ContainerId, out var container) ? container.Pos : b.Pos,
            Hp = b.Hp, MaxHp = MaxHp(b), Completed = b.Complete, BuildTicksLeft = own ? b.BuildLeft : 0, BuildTicksTotal = own ? b.BuildTotal : 0,
            BuilderId = own ? b.BuilderId : 0, Activity = own ? b.Activity : EntityActivity.Idle, Destination = own ? b.Destination : default,
            TargetId = own ? b.TargetId : 0, Cargo = own ? b.Cargo : 0, SuppliesLeft = b.RoleId == "map.dock" ? b.Supplies : 0, LoadingEntityId = own ? b.LoadingId : 0,
            ContainerId = own ? b.ContainerId : 0, OccupantIds = own ? b.Occupants.ToArray() : Array.Empty<int>(), CaptureTicksLeft = own ? b.CaptureLeft : 0,
            CaptureTicksTotal = own && b.CaptureLeft > 0 ? C.Rules.CaptureTicks : 0, VeterancyRank = b.Rank, Experience = own ? b.Experience : 0,
            Powered = b.Powered, StatusIds = statuses.ToArray(), RallyPoint = own ? b.Rally : default,
            Queue = own ? b.Queue.Select(q => new QueueItemSnapshot(q.RoleId, q.Paid, q.Left, q.Total)).ToArray() : Array.Empty<QueueItemSnapshot>()
        };
    }
    public MatchSnapshot Snapshot(int viewerSlot)
    {
        if (!Players.TryGetValue(viewerSlot, out var player)) throw new ArgumentException("Viewer must be an active match slot.");
        var visible = Bodies.Values.Where(b => b.Owner == viewerSlot || b.ContainerId == 0 && Visible(viewerSlot, b.Pos)).Select(b => Describe(b, b.Owner == viewerSlot)).ToList();
        var ids = visible.Select(e => e.Id).ToHashSet();
        visible.AddRange(player.Memory.Values.Where(e => !ids.Contains(e.Id) && !Visible(viewerSlot, e.Position)).Select(e => Clone(e)));
        return new MatchSnapshot
        {
            Tick = Tick, ViewerSlot = viewerSlot, Phase = phase, Paused = paused, WinningSlots = (int[])winners.Clone(), EliminatedSlots = Players.Values.Where(p => p.Eliminated).Select(p => p.Slot).ToArray(),
            Player = new PlayerSnapshot { Slot = viewerSlot, Money = player.Money, PowerSupply = player.Supply, PowerDemand = player.Demand, LowPower = player.LowPower, Radar = player.Radar, Eliminated = player.Eliminated, Upgrades = player.Upgrades.ToArray() },
            Entities = visible.OrderBy(e => e.Id).ToArray(), Visibility = (Visibility[])player.Fog.Clone(),
            Projectiles = shots.Where(s => Visible(viewerSlot, s.Pos)).Select(s => new ProjectileSnapshot(s.Id, s.Owner, Bodies.TryGetValue(s.Source, out var source) && (source.Owner == viewerSlot || Visible(viewerSlot, source.Pos)) ? s.Source : 0,
                Visible(viewerSlot, s.TargetPos) ? s.Target : 0, s.DamageId, s.Pos, Visible(viewerSlot, s.TargetPos) ? s.TargetPos : s.Pos)).ToArray(),
            CombatTraces = traces.Where(t => Visible(viewerSlot, t.Position)).Select(t => FilterTrace(viewerSlot, t)).OrderBy(t => t.Id).ThenBy(t => t.Phase).ToArray(),
            Events = events.Where(e => e.Slot == viewerSlot && (e.Id.StartsWith("vo.", StringComparison.Ordinal) || Visible(viewerSlot, e.Position))).ToArray()
        };
    }
    private CombatTrace FilterTrace(int viewerSlot, CombatTrace t)
    {
        int sourceId = Bodies.TryGetValue(t.SourceId, out var source) && (source.Owner == viewerSlot || Visible(viewerSlot, source.Pos)) ? t.SourceId : 0;
        bool targetVisible = Visible(viewerSlot, t.TargetPosition);
        return t with
        {
            SourceId = sourceId,
            TargetId = targetVisible ? t.TargetId : 0,
            TargetPosition = targetVisible ? t.TargetPosition : t.Position
        };
    }
}
