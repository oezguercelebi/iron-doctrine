using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

public sealed class MatchFactory : IMatchFactory
{
    public IMatch Create(GameConfig config, MatchSetup setup) => new Match(config, setup);
}

// The entire authoritative match is plain data, advanced by one integer tick.
internal sealed partial class Match : IMatch
{
    internal readonly GameConfig C;
    internal readonly MatchSetup S;
    internal readonly SortedDictionary<int, Body> Bodies = new();
    internal readonly SortedDictionary<int, Player> Players = new();
    private readonly List<MatchOrder> pending = new();
    private readonly List<Shot> shots = new();
    private readonly List<MatchEvent> events = new();
    private readonly SortedDictionary<int, AiBrain> brains = new();
    private readonly Dictionary<string, RoleConfig> roles;
    private int nextId = 1, nextShotId = 1, topology;
    private uint random;
    private bool paused;
    private MatchPhase phase;
    private int[] winners = Array.Empty<int>();
    public long Tick { get; private set; }
    public GameConfig Config => Clone(C);
    public MatchSetup Setup => Clone(S);
    internal static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;

    internal Match(GameConfig config, MatchSetup setup)
    {
        C = Clone(config); C.Validate(); S = Clone(setup);
        if (S.Slots.Length != CatalogIds.MaxSlots || S.Slots.Select(s => s.Index).Distinct().Count() != CatalogIds.MaxSlots || S.Slots.Any(s => s.Index < 0 || s.Index >= CatalogIds.MaxSlots)) throw new ArgumentException("Match needs eight distinct slots.");
        if (S.ModeId != "mode.skirmish" || S.MapId != C.Map.Id || S.Superweapons || S.Crates) throw new ArgumentException("Unsupported slice settings.");
        var active = S.Slots.Where(s => s.Occupant is Occupant.Player or Occupant.AI).OrderBy(s => s.Index).ToArray();
        if (active.Length == 0 || active.Length > C.Map.MaxPlayers) throw new ArgumentException("Active slots exceed this map.");
        roles = C.Roles.ToDictionary(r => r.Id, StringComparer.Ordinal);
        random = S.Seed;
        foreach (var slot in active)
        {
            if (slot.LoadoutId != "aegis.vanilla") throw new ArgumentException("This slice supports aegis.vanilla only.");
            if (!C.Map.Starts.Any(s => s.Slot == slot.Index)) throw new ArgumentException("Active slot has no map start.");
            if (slot.Occupant == Occupant.AI && slot.AiId != "ai.medium") throw new ArgumentException("This slice supports ai.medium only.");
            Players.Add(slot.Index, new Player { Slot = slot.Index, Money = C.Rules.StartingCash, Fog = new Visibility[C.Map.WidthCells * C.Map.HeightCells] });
            if (slot.Occupant == Occupant.AI) brains.Add(slot.Index, new AiBrain(slot.Index));
        }
        foreach (var item in C.Map.Objects) Spawn(item.RoleId, -1, item.Position).Supplies = item.Supplies;
        foreach (var slot in active)
        {
            var start = C.Map.Starts.Single(s => s.Slot == slot.Index);
            Spawn("prod.command", slot.Index, start.Command);
            Spawn("build.dozer", slot.Index, start.Builder);
        }
        UpdatePower(); UpdateFog();
    }

    internal Body Spawn(string roleId, int owner, WorldPoint position, bool complete = true)
    {
        var r = roles[roleId];
        var body = new Body { Id = nextId++, RoleId = roleId, Owner = owner, Pos = position, Destination = position, Rally = position, Hp = r.Hp, Complete = complete, FreeGathererGranted = complete, BuildLeft = complete ? 0 : r.BuildTicks, BuildTotal = r.BuildTicks };
        Bodies.Add(body.Id, body);
        if (Obstacle(body)) topology++;
        return body;
    }
    internal RoleConfig Role(Body body) => roles[body.RoleId];
    private bool Obstacle(Body body) => Role(body).IsBuilding || body.RoleId == "map.dock";
    internal bool Allied(int left, int right) => left >= 0 && right >= 0 && (left == right || S.Slots.Single(s => s.Index == left).Team > 0 && S.Slots.Single(s => s.Index == left).Team == S.Slots.Single(s => s.Index == right).Team);
    private bool Enemy(int left, int right) => left >= 0 && right >= 0 && !Allied(left, right);
    internal int MaxHp(Body body) => Role(body).Hp * C.Ranks[body.Rank].HpPercent / 100;
    internal static long Distance2(WorldPoint a, WorldPoint b) => (long)(a.X - b.X) * (a.X - b.X) + (long)(a.Z - b.Z) * (a.Z - b.Z);
    private static bool Near(WorldPoint a, WorldPoint b, int range) => Distance2(a, b) <= (long)range * range;
    private static int Root(long n)
    {
        ulong x = (ulong)Math.Max(0, n), result = 0, bit = 1UL << 62;
        while (bit > x) bit >>= 2;
        while (bit != 0) { if (x >= result + bit) { x -= result + bit; result = (result >> 1) + bit; } else result >>= 1; bit >>= 2; }
        return (int)result;
    }
    internal uint NextRandom() { random = unchecked(random * 1664525u + 1013904223u); return random; }
    private void Event(string id, int slot, Body? body = null) => events.Add(new MatchEvent(Tick, events.Count, id, slot, body?.Id ?? 0, body?.Pos ?? default));

    public void Step()
    {
        if (paused || phase == MatchPhase.Finished) return;
        Tick++; events.Clear();
        var orders = pending.ToArray(); pending.Clear();
        foreach (var order in orders) if (Validate(order) == "") Execute(order);
        UpdatePower();
        foreach (var body in Bodies.Values.ToArray()) if (Bodies.ContainsKey(body.Id)) StepBody(body);
        StepShots();
        UpdatePower(); UpdateFog(); CheckOutcome();
        if (phase == MatchPhase.Running) foreach (var brain in brains.Values) if (!Players[brain.Slot].Eliminated) brain.Think(this);
    }
    public void SetPaused(bool value) { if (S.LocalPauseAllowed && phase == MatchPhase.Running) paused = value; }
    private void CheckOutcome()
    {
        foreach (var player in Players.Values)
            if (!player.Eliminated && !Bodies.Values.Any(b => b.Owner == player.Slot && Role(b).IsBuilding)) { player.Eliminated = true; Event("vo.defeat", player.Slot); }
        var survivors = Players.Values.Where(p => !p.Eliminated).Select(p => p.Slot).ToArray();
        if (survivors.Length == 0 || survivors.All(a => survivors.All(b => Allied(a, b))))
        {
            phase = MatchPhase.Finished; winners = survivors;
            foreach (var slot in winners) Event("vo.victory", slot);
        }
    }
    public string StateHash()
    {
        // Public fields are explicitly included: paths, timers, orders and stale intel all affect future ticks.
        var options = new JsonSerializerOptions { IncludeFields = true };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new { C, S, Tick, random, nextId, nextShotId, topology, paused, phase, winners, pending, shots, events, Bodies, Players, brains }, options);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    internal sealed class Body
    {
        public int Id, Owner, Hp, BuildLeft, BuildTotal, BuilderId, ConstructionId;
        public string RoleId = "";
        public WorldPoint Pos, Destination, Rally;
        public bool Complete, Powered = true, FreeGathererGranted, AutoGather = true;
        public EntityActivity Activity;
        public int TargetId, Cargo, Supplies, LoadingId, ContainerId, CaptureLeft, Timer, Cooldown, Rank, Experience, GatherDockId;
        public long LastHit = long.MinValue / 2;
        public readonly List<int> Occupants = new();
        public readonly List<Production> Queue = new();
        public readonly List<MatchOrder> Actions = new();
        public readonly List<WorldPoint> Path = new();
        public WorldPoint PathGoal, PathEnd;
        public bool PathFallback;
        public int PathTopology = -1, PathRange, PathWait;
    }
    internal sealed class Production
    {
        public string RoleId = "";
        public int Paid, Left, Total;
    }
    internal sealed class Player
    {
        public int Slot, Money, Supply, Demand;
        public bool LowPower, Radar, Eliminated;
        public readonly SortedSet<string> Upgrades = new(StringComparer.Ordinal);
        public Visibility[] Fog = Array.Empty<Visibility>();
        public readonly SortedDictionary<int, EntitySnapshot> Memory = new();
    }
    private sealed class Shot
    {
        public int Id, Owner, Source, Target, Damage, Speed;
        public string DamageId = "";
        public WorldPoint Pos, TargetPos;
    }
}
