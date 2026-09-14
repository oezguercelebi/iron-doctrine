#if CLIENT_INPUT_PROOF
using IronDoctrine.Client;
using IronDoctrine.Contracts;
using IronDoctrine.Sim;

var config = GameConfig.Load(args[0]);
var factory = new MatchFactory();
var match = factory.Create(config, config.CreateSetup());
var start = match.Snapshot(0);
var builder = start.Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
var clicked = new WorldPoint(builder.Position.X + config.Map.CellSize, builder.Position.Z + config.Map.CellSize / 2);
Require(match.CanPlace(0, builder.Id, "power.fusion", clicked).Allowed, "Build point must be legal before submission.");
var build = CommandIntent.Create(0, OrderKind.Build, new[] { builder.Id }, builder.Id, clicked, "power.fusion");
Require(build.TargetId == 0 && build.Position == clicked, "A picked unit must not replace the approved build point.");
Require(match.Submit(build).Accepted, "Build order must be accepted.");
EntitySnapshot? site = null;
for (int i = 0; i < config.Role("power.fusion").BuildTicks && site == null; i++)
{
    match.Step();
    site = match.Snapshot(0).Entities.FirstOrDefault(e => e.OwnerSlot == 0 && e.RoleId == "power.fusion");
}
Require(site != null && site.Position == clicked && site.Position != builder.Position, "Actual simulation construction must use the legal ghost point, not the picked unit position.");
Console.WriteLine($"PASS real-sim Build picked={builder.Id}@{builder.Position} ghost={clicked} actual={site!.Position}");

var groundKinds = new[] { OrderKind.Build, OrderKind.Move, OrderKind.AttackMove, OrderKind.Waypoint, OrderKind.Rally, OrderKind.Exit };
foreach (var kind in groundKinds)
{
    var order = CommandIntent.Create(0, kind, new[] { builder.Id }, builder.Id, clicked, append: true);
    Require(order.TargetId == 0 && order.Position == clicked && order.Append, $"{kind} must preserve the ground point and append modifier.");
}
Console.WriteLine("PASS Build/Move/AttackMove/Waypoint/Rally/Exit discard incidental picked entity IDs");
var guardFollow = CommandIntent.Create(0, OrderKind.Guard, new[] { builder.Id }, builder.Id, clicked, append: true);
Require(guardFollow.TargetId == builder.Id && guardFollow.Position == clicked && guardFollow.Append, "Guard with a picked unit must retain TargetId so the unit is followed.");
Require(CommandIntent.Create(0, OrderKind.Guard, new[] { builder.Id }, 0, clicked).TargetId == 0, "Guard with no pick stays a ground point.");
Require(CommandIntent.Create(0, OrderKind.Move, new[] { builder.Id }, builder.Id, clicked).TargetId == 0, "Move still discards a picked entity ID.");
Console.WriteLine("PASS Guard retains a picked unit TargetId; ground Guard and Move stay ground targeting");
foreach (var kind in new[] { OrderKind.Attack, OrderKind.ForceAttack, OrderKind.Repair, OrderKind.Gather, OrderKind.Enter, OrderKind.Capture })
    Require(CommandIntent.Create(0, kind, new[] { builder.Id }, builder.Id, clicked).TargetId == builder.Id, $"{kind} must retain a deliberate target.");
Require(CommandIntent.Create(0, OrderKind.ForceAttack, new[] { builder.Id }, position: clicked).TargetId == 0, "Ground force-attack must remain ground targeting.");
Console.WriteLine("PASS entity actions retain explicit targets; ground ForceAttack remains ground targeting");

foreach (var roleId in new[] { "map.garrison", "eco.chinook", "veh.scout_gun" })
{
    var enemy = new EntitySnapshot { Id = 40, OwnerSlot = 1, RoleId = roleId, VeterancyRank = 2 };
    var role = config.Role(roleId);
    Require(SelectionIntel.Status(enemy, 0) == "STATUS UNKNOWN", "Redacted activity must not be presented as Idle.");
    var passengers = SelectionIntel.Passengers(enemy, role, 0);
    Require(passengers.StartsWith("Passengers unknown") && !passengers.Contains("0/"), "Redacted occupants must not be presented as an empty transport.");
    Require(!SelectionIntel.Veterancy(enemy, 0).Contains("XP"), "Redacted experience must be omitted.");
    Require(SelectionIntel.Veterancy(enemy, 0).Contains("ELITE"), "Public veterancy must remain visible.");
    if (role.CargoCapacity > 0) Require(passengers.Contains("Cargo unknown") && !passengers.Contains("$0"), "Redacted cargo must not be presented as zero cash.");
    Require(SelectionIntel.Status(enemy with { IsRemembered = true }, 0) == "LAST SEEN", "Remembered entities remain last-seen reports.");
}
Console.WriteLine("PASS enemy garrison/Gatherer/Scout activity, passengers, cargo and XP are unknown or omitted; public rank remains");
var owned = new EntitySnapshot { OwnerSlot = 0, RoleId = "eco.chinook", Activity = EntityActivity.Returning, OccupantIds = new[] { 1, 2 }, Cargo = config.Role("eco.chinook").CargoCapacity, Experience = 300, VeterancyRank = 1 };
Require(SelectionIntel.Status(owned, 0) == "RETURNING", "Owned activity remains available.");
Require(SelectionIntel.Passengers(owned, config.Role(owned.RoleId), 0).Contains($"2/{config.Role(owned.RoleId).Capacity}"), "Owned passenger counts remain available.");
Require(SelectionIntel.Veterancy(owned, 0).Contains("300 XP"), "Owned experience remains available.");
Console.WriteLine("PASS own private status, passenger count, cargo and XP remain available");
Console.WriteLine("CLIENT_R2_TARGETED_PROOF_OK");

static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

#endif
