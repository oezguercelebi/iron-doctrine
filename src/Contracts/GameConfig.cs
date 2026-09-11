using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronDoctrine.Contracts;

public sealed record GameConfig
{
    public string Notice { get; init; } = "";
    public RulesConfig Rules { get; init; } = new();
    public AiConfig Ai { get; init; } = new();
    public MapConfig Map { get; init; } = new();
    public SlotSetup[] DefaultSlots { get; init; } = Array.Empty<SlotSetup>();
    public uint DefaultSeed { get; init; }
    public RoleConfig[] Roles { get; init; } = Array.Empty<RoleConfig>();
    public DamageConfig[] Damage { get; init; } = Array.Empty<DamageConfig>();
    public RankConfig[] Ranks { get; init; } = Array.Empty<RankConfig>();
    public RoleConfig Role(string id) => Roles.First(r => r.Id == id);
    public MatchSetup CreateSetup() => new() { Seed = DefaultSeed, Slots = (SlotSetup[])DefaultSlots.Clone(), MapId = Map.Id };
    public static GameConfig Load(string path) => FromJson(File.ReadAllText(path));
    public static GameConfig FromJson(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        var config = JsonSerializer.Deserialize<GameConfig>(json, options) ?? throw new InvalidDataException("Missing config");
        config.Validate();
        return config;
    }
    public void Validate()
    {
        if (!Notice.Contains("placeholder", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Tuning must be marked placeholder");
        if (Rules.TickRate <= 0 || Rules.UnitsPerWorldUnit <= 0 || Map.CellSize <= 0 || Map.WidthCells <= 0 || Map.HeightCells <= 0) throw new InvalidDataException("Invalid world clock or grid");
        if (DefaultSlots.Length != CatalogIds.MaxSlots || DefaultSlots.Select(s => s.Index).Distinct().Count() != CatalogIds.MaxSlots || DefaultSlots.Any(s => s.Index < 0 || s.Index >= CatalogIds.MaxSlots || !CatalogIds.Loadouts.Contains(s.LoadoutId))) throw new InvalidDataException("Preserve eight distinct slots and catalog loadouts");
        if (Map.MaxPlayers < 2 || Map.MaxPlayers > CatalogIds.MaxSlots || Map.Starts.Length != Map.MaxPlayers) throw new InvalidDataException("Map start count");
        if (Roles.Select(r => r.Id).Distinct().Count() != Roles.Length || Roles.Any(r => !CatalogIds.SliceRoles.Contains(r.Id))) throw new InvalidDataException("Only distinct slice roles may have tuning");
        foreach (var id in CatalogIds.SliceRoles) if (!Roles.Any(r => r.Id == id)) throw new InvalidDataException("Missing slice role " + id);
        if (Roles.Any(r => r.Radius < 0 || r.Cost < 0 || r.BuildTicks < 0)) throw new InvalidDataException("Negative tuning");
    }
}
public sealed record RulesConfig
{
    public int TickRate { get; init; }
    public int UnitsPerWorldUnit { get; init; }
    public int StartingCash { get; init; }
    public int HardEntityCap { get; init; }
    public int QueueLimit { get; init; }
    public int SellRefundPercent { get; init; }
    public int QueueRefundPercent { get; init; }
    public int ConstructionCancelRefundPercent { get; init; }
    public int InteractionRange { get; init; }
    public int RepairHpPerTick { get; init; }
    public int RepairCostPerHp { get; init; }
    public int FactoryRepairHpPerTick { get; init; }
    public int FactoryRepairRange { get; init; }
    public int CaptureTicks { get; init; }
    public int GatherLoadTicks { get; init; }
    public int GatherUnloadTicks { get; init; }
    public int SpawnOffset { get; init; }
    public int GarrisonSpillHpPercent { get; init; }
    public int CrushRadius { get; init; }
    public int MissileHitRadius { get; init; }
    public int GroundForceAttackRadius { get; init; }
    public int GuardLeash { get; init; }
    public int PathReplanTicks { get; init; }
    public int SelfHealDelayTicks { get; init; }
}
public sealed record AiConfig
{
    public int ThinkTicks { get; init; }
    public int AttackGroupSize { get; init; }
    public int AttackDelayTicks { get; init; }
    public int DefendRadius { get; init; }
    public int DesiredGatherers { get; init; }
    public int BuildSpacing { get; init; }
    public int PlacementSearchCells { get; init; }
    public string[] BuildOrder { get; init; } = Array.Empty<string>();
    public string[] Composition { get; init; } = Array.Empty<string>();
}
public sealed record RoleConfig
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = ""; // Provisional role label only.
    public string Model { get; init; } = ""; // res://assets/models/....glb; authored metre scale.
    public bool IsBuilding { get; init; }
    public bool IsFlying { get; init; }
    public bool IsInfantry { get; init; }
    public bool IsNeutral { get; init; }
    public bool Indestructible { get; init; }
    public int Hp { get; init; }
    public int Cost { get; init; }
    public int BuildTicks { get; init; }
    public int Radius { get; init; }
    public int SpeedPerTick { get; init; }
    public int SightRange { get; init; }
    public int PowerSupply { get; init; }
    public int PowerDrain { get; init; }
    public string ArmorId { get; init; } = "";
    public string DamageId { get; init; } = "";
    public string DeliveryId { get; init; } = "";
    public int Damage { get; init; }
    public int AttackRange { get; init; }
    public int AttackCooldownTicks { get; init; }
    public int ProjectileSpeedPerTick { get; init; }
    public int Capacity { get; init; }
    public int CargoCapacity { get; init; }
    public bool FireFromTransport { get; init; }
    public int KillExperience { get; init; }
    public string ProducerId { get; init; } = "";
    public string[] Prerequisites { get; init; } = Array.Empty<string>();
    public string[] AbilityIds { get; init; } = Array.Empty<string>();
}
public sealed record DamageConfig
{
    public string Id { get; init; } = "";
    // Percent of role damage. Zero prohibits targeting that armor. Occupied garrison uses arm.garrison.
    public Dictionary<string, int> ArmorPercent { get; init; } = new();
}
public sealed record RankConfig
{
    public int Experience { get; init; }
    public int DamagePercent { get; init; }
    public int CooldownPercent { get; init; }
    public int HpPercent { get; init; }
    public int HealHpPerTick { get; init; }
}
public sealed record MapConfig
{
    public string Id { get; init; } = "maps.two_flats";
    public string LayoutId { get; init; } = "layout.duel";
    public int MaxPlayers { get; init; }
    public int WidthCells { get; init; }
    public int HeightCells { get; init; }
    public int CellSize { get; init; }
    public StartConfig[] Starts { get; init; } = Array.Empty<StartConfig>();
    // Default ter.ground; rectangles override cell type. ter.unbuildable is impassable on ground in this map.
    public TerrainRect[] Terrain { get; init; } = Array.Empty<TerrainRect>();
    public MapObjectConfig[] Objects { get; init; } = Array.Empty<MapObjectConfig>();
    public string TerrainAt(int x, int z)
    {
        if (x < 0 || z < 0 || x >= WidthCells || z >= HeightCells) return "ter.unbuildable";
        var id = "ter.ground";
        foreach (var rect in Terrain) if (x >= rect.X && z >= rect.Z && x < rect.X + rect.Width && z < rect.Z + rect.Height) id = rect.TerrainId;
        return id;
    }
}
public sealed record StartConfig(int Slot, WorldPoint Command, WorldPoint Builder);
public sealed record TerrainRect(int X, int Z, int Width, int Height, string TerrainId);
public sealed record MapObjectConfig(string RoleId, WorldPoint Position, int Supplies, string[] Tags);
