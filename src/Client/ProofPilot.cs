using IronDoctrine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

/// <summary>
/// Opt-in visible play proof. It is a player at the same public seam as mouse input:
/// only its own fog snapshot, public setup/config, CanPlace and ordinary orders.
/// Timings, economy, builds and unit capabilities are unchanged for both slots.
/// </summary>
internal sealed class ProofPilot
{
    private readonly IMatch _match;
    private readonly int _slot;
    private readonly Action<MatchOrder> _send;
    private readonly GameConfig _config;
    private readonly WorldPoint _home;
    private readonly WorldPoint[] _opponentStarts;
    private long _lastThink = -1;
    private long _lastAttack;
    private int _composition;
    public int OrdersIssued { get; private set; }

    public ProofPilot(IMatch match, int slot, Action<MatchOrder> send)
    {
        _match = match; _slot = slot; _send = send; _config = match.Config;
        _home = _config.Map.Starts.First(s => s.Slot == slot).Command;
        var us = match.Setup.Slots.First(s => s.Index == slot);
        _opponentStarts = _config.Map.Starts.Where(start => match.Setup.Slots.Any(s => s.Index == start.Slot && s.Index != slot && s.Occupant is Occupant.Player or Occupant.AI && (us.Team == 0 || s.Team != us.Team))).Select(s => s.Command).ToArray();
    }

    public void Think(MatchSnapshot snapshot)
    {
        if (snapshot.Phase != MatchPhase.Running || snapshot.Paused || _lastThink >= 0 && snapshot.Tick - _lastThink < _config.Ai.ThinkTicks) return;
        _lastThink = snapshot.Tick;
        var own = snapshot.Entities.Where(e => e.OwnerSlot == _slot).ToArray();
        int budget = snapshot.Player.Money;
        var builder = own.FirstOrDefault(e => e.RoleId == "build.dozer" && e.Activity == EntityActivity.Idle);
        if (!own.Any(e => e.RoleId == "build.dozer"))
        {
            var command = own.FirstOrDefault(e => e.RoleId == "prod.command" && e.Completed && e.Queue.Length == 0);
            if (command != null) Queue(command, "build.dozer", ref budget);
        }
        if (builder != null)
        {
            var needed = new Dictionary<string, int>();
            foreach (string roleId in _config.Ai.BuildOrder)
            {
                needed.TryGetValue(roleId, out int count);
                needed[roleId] = ++count;
                if (own.Count(e => e.RoleId == roleId) >= count) continue;
                var role = _config.Role(roleId);
                if (budget < role.Cost || role.Prerequisites.Any(p => !own.Any(e => e.RoleId == p && e.Completed))) break;
                var point = FindPlacement(builder.Id, roleId);
                if (point.HasValue)
                {
                    Send(new MatchOrder(_slot, OrderKind.Build, new[] { builder.Id }, Position: point.Value, ProductId: roleId));
                    budget -= role.Cost;
                }
                break;
            }
        }
        foreach (var gatherer in own.Where(e => e.RoleId == "eco.chinook" && e.Activity == EntityActivity.Idle && e.OccupantIds.Length == 0))
        {
            var dock = snapshot.Entities.Where(e => e.RoleId == "map.dock" && e.SuppliesLeft > 0 && !e.IsRemembered).OrderBy(e => Distance(e.Position, gatherer.Position)).FirstOrDefault();
            if (dock != null) Send(new MatchOrder(_slot, OrderKind.Gather, new[] { gatherer.Id }, dock.Id, dock.Position));
        }
        if (own.Count(e => e.RoleId == "eco.chinook") < _config.Ai.DesiredGatherers)
        {
            var dropoff = own.FirstOrDefault(e => e.RoleId == "eco.dropoff" && e.Completed && e.Queue.Length == 0);
            if (dropoff != null) Queue(dropoff, "eco.chinook", ref budget);
        }
        foreach (var producer in own.Where(e => e.Completed && e.Queue.Length < 2 && _config.Role(e.RoleId).IsBuilding))
        {
            var pool = _config.Ai.Composition.Where(id => _config.Role(id).ProducerId == producer.RoleId).ToArray();
            if (pool.Length > 0) Queue(producer, pool[_composition++ % pool.Length], ref budget);
        }
        var army = own.Where(e => e.ContainerId == 0 && !_config.Role(e.RoleId).IsBuilding && _config.Role(e.RoleId).Damage > 0).ToArray();
        bool timeToAttack = snapshot.Tick >= _config.Ai.AttackDelayTicks && army.Length >= _config.Ai.AttackGroupSize;
        if (timeToAttack && snapshot.Tick - _lastAttack >= _config.Ai.ThinkTicks * 8L)
        {
            var hostile = snapshot.Entities.Where(e => e.OwnerSlot >= 0 && e.OwnerSlot != _slot && !Allied(e.OwnerSlot) && _config.Role(e.RoleId).IsBuilding).OrderBy(e => Distance(e.Position, _home)).FirstOrDefault();
            WorldPoint target = hostile?.Position ?? (_opponentStarts.Length > 0 ? _opponentStarts[0] : _home);
            Send(new MatchOrder(_slot, OrderKind.AttackMove, army.Select(e => e.Id).ToArray(), Position: target));
            _lastAttack = snapshot.Tick;
        }
    }

    private bool Allied(int other)
    {
        var us = _match.Setup.Slots.First(s => s.Index == _slot);
        return us.Team > 0 && _match.Setup.Slots.Any(s => s.Index == other && s.Team == us.Team);
    }
    private WorldPoint? FindPlacement(int builder, string role)
    {
        int spacing = _config.Ai.BuildSpacing;
        int cells = _config.Ai.PlacementSearchCells;
        for (int ring = 1; ring <= cells; ring++)
        for (int z = -ring; z <= ring; z++)
        for (int x = -ring; x <= ring; x++)
        {
            if (Math.Abs(x) != ring && Math.Abs(z) != ring) continue;
            var point = new WorldPoint(_home.X + x * spacing, _home.Z + z * spacing);
            if (point.X <= 0 || point.Z <= 0 || point.X >= _config.Map.WidthCells * _config.Map.CellSize || point.Z >= _config.Map.HeightCells * _config.Map.CellSize) continue;
            if (_match.CanPlace(_slot, builder, role, point).Allowed) return point;
        }
        return null;
    }
    private void Queue(EntitySnapshot producer, string role, ref int budget)
    {
        int cost = _config.Role(role).Cost;
        if (budget < cost) return;
        Send(new MatchOrder(_slot, OrderKind.Queue, new[] { producer.Id }, ProductId: role));
        budget -= cost;
    }
    private void Send(MatchOrder order) { OrdersIssued++; _send(order); }
    private static long Distance(WorldPoint a, WorldPoint b) { long x = a.X - b.X, z = a.Z - b.Z; return x * x + z * z; }
}
