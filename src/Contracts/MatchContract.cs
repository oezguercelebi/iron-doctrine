using System;
using System.Collections.Generic;

namespace IronDoctrine.Contracts;

// All positions/distances are integer simulation units. Divide by Config.Rules.UnitsPerWorldUnit for view coordinates.
public readonly record struct WorldPoint(int X, int Z);
public enum Occupant { Closed, Open, Player, AI }
public enum OrderKind { Move, Attack, AttackMove, Stop, Guard, Waypoint, ForceAttack, Build, Rally, Queue, CancelQueue, Sell, Repair, Gather, Enter, Exit, Capture, Resign }
public enum MatchPhase { Running, Finished }
public enum Visibility : byte { Shroud, Fog, Visible }
public enum EntityActivity { Idle, Moving, Attacking, Building, Gathering, Returning, Loading, Waiting, Repairing, Capturing, Contained }

public sealed record SlotSetup
{
    public int Index { get; init; }
    public Occupant Occupant { get; init; }
    // 0 = FFA; otherwise same positive team is allied. Closed/Open slots never spawn.
    public int Team { get; init; }
    public string LoadoutId { get; init; } = "aegis.vanilla";
    public string Color { get; init; } = "#FFFFFF";
    public string AiId { get; init; } = "ai.medium";
}
public sealed record MatchSetup
{
    public uint Seed { get; init; }
    public string ModeId { get; init; } = "mode.skirmish";
    public string MapId { get; init; } = "maps.two_flats";
    public SlotSetup[] Slots { get; init; } = Array.Empty<SlotSetup>();
    public bool Fog { get; init; } = true;
    public bool Superweapons { get; init; }
    public bool Crates { get; init; }
    public bool LocalPauseAllowed { get; init; } = true;
}

// Submitted orders execute in submission order at the next Step. ActorIds must be owned by Slot.
// Waypoint always appends; Append makes other unit movement/action orders append. Queue always appends production.
// Exit: actors are containers (all occupants exit) or contained owned infantry (only those exit).
// CancelQueue: QueueIndex indexes the producer's snapshot Queue. ProductId is catalog id for Build/Queue.
// ForceAttack may name an entity or a ground position; Attack requires currently visible hostile target.
public sealed record MatchOrder(int Slot, OrderKind Kind, int[] ActorIds, int TargetId = 0,
    WorldPoint Position = default, string ProductId = "", bool Append = false, int QueueIndex = 0);
public sealed record OrderReceipt(bool Accepted, string Reason);
public sealed record PlacementResult(bool Allowed, string Reason);

public interface IMatchFactory { IMatch Create(GameConfig config, MatchSetup setup); }
public interface IMatch
{
    GameConfig Config { get; }
    MatchSetup Setup { get; }
    long Tick { get; }
    OrderReceipt Submit(MatchOrder order);
    void Step();
    void SetPaused(bool paused);
    MatchSnapshot Snapshot(int viewerSlot);
    // No information leak: checks terrain, known building footprints, ownership/prereq/funds.
    // Actual construction checks all physical collisions when executing the queued order.
    PlacementResult CanPlace(int slot, int builderId, string roleId, WorldPoint position);
    // Complete deterministic internal state incl. RNG, pending orders, queues, fog and AI memory. Test use only.
    string StateHash();
}

public sealed record QueueItemSnapshot(string ProductId, int PaidCost, int RemainingTicks, int TotalTicks);
public sealed record EntitySnapshot
{
    // Entity id 0 reserved for no target; neutral owner is -1. Snapshot values must be detached from mutable sim state.
    public int Id { get; init; }
    public string RoleId { get; init; } = "";
    public int OwnerSlot { get; init; } = -1;
    public WorldPoint Position { get; init; }
    public WorldPoint Destination { get; init; }
    public int Hp { get; init; }
    public int MaxHp { get; init; }
    public bool Completed { get; init; }
    public int BuildTicksLeft { get; init; }
    public int BuildTicksTotal { get; init; }
    public int BuilderId { get; init; }
    public EntityActivity Activity { get; init; }
    public int TargetId { get; init; }
    public int Cargo { get; init; }
    public int SuppliesLeft { get; init; }
    public int LoadingEntityId { get; init; }
    public int ContainerId { get; init; }
    public int[] OccupantIds { get; init; } = Array.Empty<int>();
    public int CaptureTicksLeft { get; init; }
    public int CaptureTicksTotal { get; init; }
    public int VeterancyRank { get; init; }
    public int Experience { get; init; }
    public bool Powered { get; init; } = true;
    public string[] StatusIds { get; init; } = Array.Empty<string>();
    public WorldPoint RallyPoint { get; init; }
    public QueueItemSnapshot[] Queue { get; init; } = Array.Empty<QueueItemSnapshot>();
    // Remembered enemy/neutral buildings may be returned in fog; never update their stale values while unseen.
    public bool IsRemembered { get; init; }
}
public sealed record PlayerSnapshot
{
    public int Slot { get; init; }
    public int Money { get; init; }
    public int PowerSupply { get; init; }
    public int PowerDemand { get; init; }
    public bool LowPower { get; init; }
    public bool Radar { get; init; }
    public bool Eliminated { get; init; }
    public string[] Upgrades { get; init; } = Array.Empty<string>();
}
public sealed record ProjectileSnapshot(int Id, int OwnerSlot, int SourceId, int TargetId, string DamageId, WorldPoint Position, WorldPoint TargetPosition);
// Event ids are catalog feedback/voice ids; tick + ordinal are stable for deduplication. Events are current-tick only.
public sealed record MatchEvent(long Tick, int Ordinal, string Id, int Slot, int EntityId, WorldPoint Position);
public sealed record MatchSnapshot
{
    public long Tick { get; init; }
    public int ViewerSlot { get; init; }
    public MatchPhase Phase { get; init; }
    public bool Paused { get; init; }
    public int[] WinningSlots { get; init; } = Array.Empty<int>();
    public int[] EliminatedSlots { get; init; } = Array.Empty<int>();
    // Only viewer's private economy/research. Other slot identity is public in Setup.
    public PlayerSnapshot Player { get; init; } = new();
    public EntitySnapshot[] Entities { get; init; } = Array.Empty<EntitySnapshot>();
    public ProjectileSnapshot[] Projectiles { get; init; } = Array.Empty<ProjectileSnapshot>();
    // Row major z * Map.WidthCells + x; no units/minimap blips from hidden cells.
    public Visibility[] Visibility { get; init; } = Array.Empty<Visibility>();
    public MatchEvent[] Events { get; init; } = Array.Empty<MatchEvent>();
}
