using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    // Pure geometry cache; its contents are derived from topology and never choose different paths.
    private readonly Dictionary<int, (int revision, bool[] cells)> walkability = new();
    private bool[] Walkability(int radius)
    {
        if (walkability.TryGetValue(radius, out var cached) && cached.revision == topology) return cached.cells;
        var cells = new bool[C.Map.WidthCells * C.Map.HeightCells];
        var obstacles = Bodies.Values.Where(Obstacle).ToArray();
        for (int i = 0; i < cells.Length; i++)
        {
            var point = new WorldPoint(i % C.Map.WidthCells * C.Map.CellSize + C.Map.CellSize / 2, i / C.Map.WidthCells * C.Map.CellSize + C.Map.CellSize / 2);
            cells[i] = TerrainFits(point, radius) && !obstacles.Any(b => Near(point, b.Pos, radius + Role(b).Radius));
        }
        walkability[radius] = (topology, cells); return cells;
    }
    private bool TerrainFits(WorldPoint p, int radius)
    {
        if (!InBounds(new WorldPoint(p.X - radius, p.Z - radius)) || !InBounds(new WorldPoint(p.X + radius, p.Z + radius))) return false;
        int cell = C.Map.CellSize;
        for (int z = (p.Z - radius) / cell; z <= (p.Z + radius) / cell; z++)
            for (int x = (p.X - radius) / cell; x <= (p.X + radius) / cell; x++)
            {
                if (C.Map.TerrainAt(x, z) != "ter.unbuildable") continue;
                int nearX = Math.Clamp(p.X, x * cell, (x + 1) * cell), nearZ = Math.Clamp(p.Z, z * cell, (z + 1) * cell);
                if (Near(p, new WorldPoint(nearX, nearZ), radius)) return false;
            }
        return true;
    }
    private bool GroundFits(WorldPoint p, int radius, int ignoredId = 0, bool units = false, Body? mover = null)
    {
        if (!TerrainFits(p, radius)) return false;
        foreach (var b in Bodies.Values)
        {
            if (b.Id == ignoredId || b.ContainerId != 0 || Role(b).IsFlying) continue;
            if (!Obstacle(b) && !units) continue;
            if (mover?.RoleId == "armor.basic" && Role(b).IsInfantry) continue;
            if (Near(p, b.Pos, radius + Role(b).Radius)) return false;
        }
        return true;
    }
    private WorldPoint Toward(WorldPoint from, WorldPoint to, int speed)
    {
        var length = Root(Distance2(from, to));
        if (length <= speed) return to;
        return new WorldPoint(from.X + (int)((long)(to.X - from.X) * speed / length), from.Z + (int)((long)(to.Z - from.Z) * speed / length));
    }
    private bool SegmentClear(Body body, WorldPoint end)
    {
        var p = body.Pos; int stride = Math.Max(1, Math.Min(C.Map.CellSize / 2, Role(body).Radius));
        while (!Near(p, end, stride)) { p = Toward(p, end, stride); if (!GroundFits(p, Role(body).Radius, body.Id)) return false; }
        return GroundFits(end, Role(body).Radius, body.Id);
    }
    // A* routes around terrain and footprint obstacles without any player-count assumptions.
    private void PlanPath(Body body, WorldPoint target, int range)
    {
        body.Path.Clear(); body.PathFallback = false; body.PathGoal = target; body.PathRange = range; body.PathTopology = topology;
        int w = C.Map.WidthCells, h = C.Map.HeightCells, cell = C.Map.CellSize;
        int start = Math.Clamp(body.Pos.Z / cell, 0, h - 1) * w + Math.Clamp(body.Pos.X / cell, 0, w - 1);
        var frontier = new PriorityQueue<int, (long score, int tie)>();
        var cost = new Dictionary<int, int> { [start] = 0 }; var parent = new Dictionary<int, int>();
        WorldPoint Point(int index) => new(index % w * cell + cell / 2, index / w * cell + cell / 2);
        int goalRange = range;
        foreach (var obstacle in Bodies.Values.Where(Obstacle))
            if (Near(target, obstacle.Pos, Role(obstacle).Radius + Role(body).Radius))
                goalRange = Math.Max(goalRange, Role(obstacle).Radius + Role(body).Radius + cell - Root(Distance2(target, obstacle.Pos)));
        int Heuristic(WorldPoint p) => Math.Max(0, Root(Distance2(p, target)) - goalRange);
        frontier.Enqueue(start, (Heuristic(body.Pos), start));
        int end = -1, best = start, bestDistance = Heuristic(body.Pos);
        var pass = Walkability(Role(body).Radius);
        bool Fits(int index) => pass[index];
        var visited = new HashSet<int>();
        while (frontier.TryDequeue(out int current, out _))
        {
            if (!visited.Add(current)) continue;
            var p = current == start ? body.Pos : Point(current);
            var distance = Heuristic(p);
            if (distance < bestDistance) { best = current; bestDistance = distance; }
            if (Near(p, target, Math.Max(goalRange, cell))) { end = current; body.PathFallback = goalRange != range; break; }
            int x = current % w, z = current / w;
            foreach (var direction in Directions)
            {
                int nx = x + direction.X, nz = z + direction.Z;
                if (nx < 0 || nz < 0 || nx >= w || nz >= h) continue;
                int next = nz * w + nx;
                if (!Fits(next)) continue;
                if (direction.X != 0 && direction.Z != 0 && (!Fits(z * w + nx) || !Fits(nz * w + x))) continue;
                int score = cost[current] + (direction.X != 0 && direction.Z != 0 ? Root((long)cell * cell * 2) : cell);
                if (cost.TryGetValue(next, out var old) && old <= score) continue;
                cost[next] = score; parent[next] = current; frontier.Enqueue(next, ((long)score + Heuristic(Point(next)), next));
            }
        }
        if (end < 0) { end = best; body.PathFallback = true; }
        body.PathEnd = end == start ? body.Pos : Point(end);
        for (var cursor = end; cursor != start; cursor = parent[cursor]) body.Path.Add(Point(cursor));
        body.Path.Reverse();
        if (!body.PathFallback && GroundFits(target, Role(body).Radius, body.Id)) body.Path.Add(target);
    }
    private static readonly WorldPoint[] Directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1), new(1, 1), new(-1, 1), new(-1, -1), new(1, -1) };
    private enum TravelResult { Moving, Arrived, Blocked }
    private TravelResult MoveToward(Body body, WorldPoint target, int range)
    {
        body.Destination = target;
        if (Near(body.Pos, target, range)) return TravelResult.Arrived;
        var r = Role(body);
        if (r.SpeedPerTick <= 0) return TravelResult.Blocked;
        WorldPoint next;
        if (r.IsFlying) next = Toward(body.Pos, target, r.SpeedPerTick);
        else
        {
            if (body.PathWait > 0) { body.PathWait--; return TravelResult.Moving; }
            if (body.Path.Count == 0 || body.PathTopology != topology || !Near(body.PathGoal, target, C.Map.CellSize) || body.PathRange != range)
            {
                // Direct movement is a fast path for a genuinely clear segment.
                if (range == 0 && SegmentClear(body, target)) { body.PathFallback = false; body.Path.Clear(); body.Path.Add(target); body.PathGoal = target; body.PathTopology = topology; body.PathRange = range; }
                else PlanPath(body, target, range);
            }
            if (body.Path.Count == 0)
            {
                body.PathWait = C.Rules.PathReplanTicks;
                return Near(body.Pos, target, range) ? TravelResult.Arrived : TravelResult.Blocked;
            }
            next = Toward(body.Pos, body.Path[0], r.SpeedPerTick);
            if (!GroundFits(next, r.Radius, body.Id, true, body))
            {
                // Yield deterministically around transient unit traffic. Ground units never phase through walls.
                bool found = false;
                foreach (var offset in Directions)
                {
                    var alternative = new WorldPoint(body.Pos.X + offset.X * r.SpeedPerTick, body.Pos.Z + offset.Z * r.SpeedPerTick);
                    if (GroundFits(alternative, r.Radius, body.Id, true, body) && Distance2(alternative, target) <= Distance2(body.Pos, target) + (long)C.Map.CellSize * C.Map.CellSize)
                    { next = alternative; found = true; break; }
                }
                if (!found) { body.Path.Clear(); body.PathWait = C.Rules.PathReplanTicks; return TravelResult.Moving; }
            }
            if (next == body.Path[0]) body.Path.RemoveAt(0);
        }
        var previous = body.Pos; body.Pos = next;
        foreach (var passenger in body.Occupants) if (Bodies.TryGetValue(passenger, out var infant)) infant.Pos = next;
        if (body.RoleId == "armor.basic" && previous != next)
            foreach (var victim in Bodies.Values.Where(b => b.Id != body.Id && Role(b).IsInfantry && b.ContainerId == 0 && Near(next, b.Pos, C.Rules.CrushRadius + Role(b).Radius)).ToArray()) Hit(victim, victim.Hp, body);
        if (Near(body.Pos, target, range)) return TravelResult.Arrived;
        return body.PathFallback && body.Path.Count == 0 && Near(body.Pos, body.PathEnd, Role(body).Radius) ? TravelResult.Blocked : TravelResult.Moving;
    }
    private bool ClearConstructionFootprint(Body builder, WorldPoint site, int radius)
    {
        int clearance = radius + Role(builder).Radius;
        if (!Near(builder.Pos, site, clearance)) return true;
        int offset = clearance + C.Map.CellSize;
        var access = Directions.Select(d => new WorldPoint(site.X + d.X * offset, site.Z + d.Z * offset))
            .Where(p => Near(p, site, clearance + C.Rules.InteractionRange) && GroundFits(p, Role(builder).Radius, builder.Id, true) && SegmentClear(builder, p))
            .OrderBy(p => Distance2(builder.Pos, p)).ThenBy(p => p.X).ThenBy(p => p.Z).Cast<WorldPoint?>().FirstOrDefault();
        if (access == null) { FinishAction(builder); return false; }
        MoveToward(builder, access.Value, 0);
        return !Near(builder.Pos, site, clearance);
    }
    private WorldPoint? FindFree(WorldPoint requested, RoleConfig role, int ignore = 0)
    {
        if (role.IsFlying && InBounds(requested)) return requested;
        if (GroundFits(requested, role.Radius, ignore, true)) return requested;
        var cell = C.Map.CellSize;
        for (int ring = 1; ring < Math.Max(C.Map.WidthCells, C.Map.HeightCells); ring++)
            for (int z = -ring; z <= ring; z++)
                for (int x = -ring; x <= ring; x++)
                {
                    if (Math.Abs(x) != ring && Math.Abs(z) != ring) continue;
                    var point = new WorldPoint(requested.X + x * cell, requested.Z + z * cell);
                    if (GroundFits(point, role.Radius, ignore, true)) return point;
                }
        return null;
    }
}
