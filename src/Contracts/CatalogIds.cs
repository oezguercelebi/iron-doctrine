namespace IronDoctrine.Contracts;
public static class CatalogIds
{
    public const int MaxSlots = 8;
    public static readonly string[] Loadouts = { "aegis.vanilla", "aegis.air", "aegis.laser", "aegis.super", "forge.vanilla", "forge.tank", "forge.infantry", "forge.nuke", "veil.vanilla", "veil.toxin", "veil.demo", "veil.stealth", "boss.mix" };
    public static readonly string[] Prefixes = { "ab.", "addon.", "aegis.", "forge.", "veil.", "ai.", "air.", "arm.", "armor.", "arty.", "boss.", "build.", "cam.", "ch.", "clk.", "deceive.", "def.", "del.", "det.", "dmg.", "eco.", "ent.", "field.", "fx.", "hero.", "hud.", "inf.", "job.", "layout.", "map.", "maps.", "mode.", "pow.", "power.", "prod.", "set.", "sfx.", "sight.", "st.", "stealth.", "sw.", "tag.", "ter.", "ui.", "up.", "veh.", "vo.", "win." };
    public static readonly string[] SliceRoles = { "build.dozer", "eco.dropoff", "eco.chinook", "power.fusion", "prod.command", "prod.barracks", "prod.factory", "inf.rifle", "inf.rocket", "armor.basic", "veh.scout_gun", "def.patriot", "map.dock", "map.garrison", "up.capture" };
}
