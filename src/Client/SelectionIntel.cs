using IronDoctrine.Contracts;

namespace IronDoctrine.Client;

internal static class SelectionIntel
{
    // Own live intel only. Remembered and foreign snapshots must not present private fields as facts.
    public static bool OwnLive(EntitySnapshot entity, int viewer) => entity.OwnerSlot == viewer && !entity.IsRemembered;

    public static string Status(EntitySnapshot entity, int viewer) => entity.IsRemembered ? "LAST SEEN"
        : OwnLive(entity, viewer) ? entity.Activity.ToString().ToUpperInvariant() : "STATUS UNKNOWN";

    public static string Passengers(EntitySnapshot entity, RoleConfig role, int viewer) => OwnLive(entity, viewer)
        ? $"Passengers  {entity.OccupantIds.Length}/{role.Capacity}" + (role.CargoCapacity > 0 ? $"  ·  Cargo ${entity.Cargo}" : "")
        : "Passengers unknown" + (role.CargoCapacity > 0 ? "  ·  Cargo unknown" : "");

    public static string Veterancy(EntitySnapshot entity, int viewer)
    {
        string rank = entity.VeterancyRank switch { 1 => "›  VETERAN", 2 => "››  ELITE", 3 => "›››  HEROIC", _ => "UNRANKED" };
        return rank + (OwnLive(entity, viewer) ? $"  ·  {entity.Experience} XP" : "");
    }
}
