using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

/// <summary>
/// Opt-in anomaly log. Off by default so StateHash and normal play are unchanged.
/// Enable with IRON_DIAG=1, Godot --diag, or BehaviorAudit.
/// </summary>
internal static class BehaviorLog
{
    public static bool Enabled { get; set; }
    public static readonly List<string> Lines = new();
    public static int StuckEvents, OscillateEvents, FallbackEvents, BoxEvents;
    public static readonly Dictionary<string, int> Jobs = new(StringComparer.Ordinal);

    private static readonly Dictionary<int, Track> tracks = new();
    private sealed class Track
    {
        public WorldPoint Pos;
        public long DestAtWindow;
        public int Still, Flips, LastDx, LastDz, Samples;
        public bool NotedStuck, NotedOsc;
    }

    public static void Reset()
    {
        Lines.Clear(); tracks.Clear(); Jobs.Clear();
        StuckEvents = OscillateEvents = FallbackEvents = BoxEvents = 0;
    }

    public static void Note(long tick, string kind, string detail)
    {
        if (!Enabled) return;
        Lines.Add($"tick={tick} {kind} {detail}");
        if (Lines.Count > 4000) Lines.RemoveRange(0, 1000);
    }

    public static void Job(int slot, long tick, string job, string detail = "")
    {
        if (!Enabled) return;
        Jobs[job] = Jobs.GetValueOrDefault(job) + 1;
        Note(tick, "ai." + job, $"slot={slot} {detail}");
    }

    public static void Box(long tick, int count, string roles, bool append)
    {
        if (!Enabled) return;
        BoxEvents++;
        Note(tick, "box", $"count={count} append={(append ? 1 : 0)} roles={roles}");
    }

    public static void Observe(Match match)
    {
        if (!Enabled) return;
        var live = new HashSet<int>();
        int moving = 0, waiting = 0, fallback = 0;
        foreach (var body in match.Bodies.Values)
        {
            var role = match.Role(body);
            if (role.IsBuilding || role.SpeedPerTick <= 0 || body.ContainerId != 0) continue;
            live.Add(body.Id);
            if (body.Activity == EntityActivity.Moving) moving++;
            if (body.PathWait > 0) waiting++;
            if (body.PathFallback) fallback++;
            Watch(match, body, role);
        }
        foreach (int id in tracks.Keys.Where(id => !live.Contains(id)).ToArray()) tracks.Remove(id);
        if (fallback > 0 && match.Tick % Math.Max(1, match.C.Rules.TickRate) == 0)
        {
            FallbackEvents += fallback;
            Note(match.Tick, "path.fallback", $"units={fallback} moving={moving} waiting={waiting}");
        }
        if (match.Tick > 0 && match.Tick % (match.C.Rules.TickRate * 10) == 0)
            Note(match.Tick, "summary",
                $"moving={moving} waiting={waiting} fallback={fallback} stuck={StuckEvents} oscillate={OscillateEvents} box={BoxEvents} ai={string.Join(',', Jobs.Select(kv => kv.Key + ':' + kv.Value))}");
    }

    private static void Watch(Match match, Match.Body body, RoleConfig role)
    {
        if (!tracks.TryGetValue(body.Id, out var track))
            tracks[body.Id] = track = new Track { Pos = body.Pos };
        int dx = Math.Sign(body.Pos.X - track.Pos.X), dz = Math.Sign(body.Pos.Z - track.Pos.Z);
        bool walking = body.Activity is EntityActivity.Moving or EntityActivity.Returning;
        long dest2 = Match.Distance2(body.Pos, body.Destination);
        bool arrived = dest2 <= (long)role.Radius * role.Radius * 4;
        if (walking && body.Pos.Equals(track.Pos) && !arrived) track.Still++;
        else track.Still = 0;
        if (walking && (dx != 0 || dz != 0) && track.Samples > 0 && (dx == -track.LastDx && dx != 0 || dz == -track.LastDz && dz != 0))
            track.Flips++;
        if (dx != 0 || dz != 0) { track.LastDx = dx; track.LastDz = dz; }
        if (track.Samples == 0) track.DestAtWindow = dest2;
        track.Samples++;
        track.Pos = body.Pos;
        int hold = Math.Max(12, match.C.Rules.PathReplanTicks * 3);
        if (walking && track.Still >= hold && !track.NotedStuck)
        {
            track.NotedStuck = true;
            StuckEvents++;
            Note(match.Tick, "stuck",
                $"id={body.Id} role={body.RoleId} slot={body.Owner} activity={body.Activity} pos={body.Pos.X},{body.Pos.Z} dest={body.Destination.X},{body.Destination.Z} wait={body.PathWait} fallback={(body.PathFallback ? 1 : 0)} still={track.Still}");
        }
        if (track.Still == 0) track.NotedStuck = false;
        if (track.Samples >= 16)
        {
            bool noProgress = dest2 >= track.DestAtWindow;
            if (walking && track.Flips >= 6 && noProgress && !arrived)
            {
                if (!track.NotedOsc)
                {
                    track.NotedOsc = true;
                    OscillateEvents++;
                    Note(match.Tick, "oscillate",
                        $"id={body.Id} role={body.RoleId} slot={body.Owner} flips={track.Flips} pos={body.Pos.X},{body.Pos.Z} dest={body.Destination.X},{body.Destination.Z}");
                }
            }
            else track.NotedOsc = false;
            track.Flips = 0;
            track.Samples = 0;
        }
    }

    public static string Report()
    {
        var text = new StringBuilder();
        text.AppendLine($"BEHAVIOR_REPORT stuck={StuckEvents} oscillate={OscillateEvents} fallback_ticks={FallbackEvents} box={BoxEvents}");
        foreach (var kv in Jobs.OrderBy(k => k.Key)) text.AppendLine($"ai.{kv.Key}={kv.Value}");
        int shown = 0;
        foreach (var line in Lines)
        {
            if (line.Contains("summary ", StringComparison.Ordinal) || line.Contains(" stuck ", StringComparison.Ordinal) || line.Contains(" oscillate ", StringComparison.Ordinal) || line.Contains(" box ", StringComparison.Ordinal) || line.Contains("ai.", StringComparison.Ordinal))
            {
                text.AppendLine(line);
                if (++shown >= 80) break;
            }
        }
        if (StuckEvents + OscillateEvents == 0) text.AppendLine("BEHAVIOR_OK no stuck or oscillate events");
        else text.AppendLine($"BEHAVIOR_ISSUES stuck={StuckEvents} oscillate={OscillateEvents}");
        return text.ToString();
    }
}
