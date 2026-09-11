using System;
using System.Collections.Generic;
using System.Linq;
using IronDoctrine.Contracts;

namespace IronDoctrine.Sim;

internal sealed partial class Match
{
    // The brain is deliberately restricted to IMatch: the same redacted snapshot and order path as the client.
    private sealed class AiBrain
    {
        public int Slot;
        public long NextThink, LastAttack;
        public int EnemyStartIndex;
        public readonly SortedDictionary<string, int> ProductionCursor = new(StringComparer.Ordinal);
        public readonly SortedSet<int> SentArmy = new();
        public AiBrain(int slot) { Slot = slot; }
        public void Think(IMatch match)
        {
            if (match.Tick < NextThink) return;
            var config = match.Config; var setup = match.Setup;
            NextThink = match.Tick + Math.Max(1, config.Ai.ThinkTicks);
            var view = match.Snapshot(Slot);
            var own = view.Entities.Where(e => e.OwnerSlot == Slot).ToArray();
            int money = view.Player.Money;
            bool Send(MatchOrder o) => match.Submit(o).Accepted;
            var start = config.Map.Starts.Single(s => s.Slot == Slot).Command;
            var dozer = own.Where(e => e.RoleId == "build.dozer").OrderBy(e => e.Id).FirstOrDefault(e => e.Activity is EntityActivity.Idle or EntityActivity.Repairing);
            var missing = MissingBuilding(config, own);
            WorldPoint? expansionAnchor = null;
            if (missing == null && view.Entities.Any(e => e.RoleId == "map.dock" && e.SuppliesLeft == 0))
            {
                var expansion = view.Entities.Where(e => e.RoleId == "map.dock" && e.SuppliesLeft > 0 && !e.IsRemembered &&
                    !own.Any(d => d.RoleId == "eco.dropoff" && Distance2(d.Position, e.Position) <= (long)config.Ai.BuildSpacing * config.Ai.BuildSpacing * 4))
                    .OrderBy(e => Distance2(start, e.Position)).FirstOrDefault();
                if (expansion != null) { missing = "eco.dropoff"; expansionAnchor = expansion.Position; }
            }
            // job.base includes replacing a lost builder and rebuilding the configured infrastructure.
            if (!own.Any(e => e.RoleId == "build.dozer"))
            {
                var command = own.FirstOrDefault(e => e.RoleId == "prod.command" && e.Completed && e.Queue.Length == 0);
                if (command != null && money >= config.Role("build.dozer").Cost && Send(new MatchOrder(Slot, OrderKind.Queue, new[] { command.Id }, ProductId: "build.dozer"))) money -= config.Role("build.dozer").Cost;
            }
            if (dozer != null && missing != null && money >= config.Role(missing).Cost)
            {
                var anchor = expansionAnchor ?? start;
                if (missing == "eco.dropoff" && expansionAnchor == null)
                {
                    var dock = config.Map.Objects.Where(o => o.RoleId == "map.dock").OrderBy(o => Distance2(start, o.Position)).First();
                    anchor = new WorldPoint((dock.Position.X + start.X) / 2, (dock.Position.Z + start.Z) / 2);
                }
                var spot = Placement(match, config, dozer.Id, missing, anchor);
                if (spot != null && Send(new MatchOrder(Slot, OrderKind.Build, new[] { dozer.Id }, Position: spot.Value, ProductId: missing))) money -= config.Role(missing).Cost;
            }
            // job.gather: target only visible docks; stale depletion never reads the authoritative supply count.
            var docks = view.Entities.Where(e => e.RoleId == "map.dock" && e.SuppliesLeft > 0 && !e.IsRemembered).ToArray();
            foreach (var gatherer in own.Where(e => e.RoleId == "eco.chinook" && e.OccupantIds.Length == 0 && e.Activity == EntityActivity.Idle))
            {
                var dock = docks.OrderBy(e => Distance2(gatherer.Position, e.Position)).ThenBy(e => e.Id).FirstOrDefault();
                if (dock != null) Send(new MatchOrder(Slot, OrderKind.Gather, new[] { gatherer.Id }, TargetId: dock.Id));
                else
                {
                    var unexplored = config.Map.Objects.Where(o => o.RoleId == "map.dock" && Cell(view, config, o.Position) == Visibility.Shroud).OrderBy(o => Distance2(gatherer.Position, o.Position)).FirstOrDefault();
                    if (unexplored != null) Send(new MatchOrder(Slot, OrderKind.Move, new[] { gatherer.Id }, Position: unexplored.Position));
                }
            }
            var count = own.Count(e => e.RoleId == "eco.chinook") + own.Sum(e => e.Queue.Count(q => q.ProductId == "eco.chinook"));
            var producer = own.FirstOrDefault(e => e.RoleId == "eco.dropoff" && e.Completed && e.Queue.Length == 0);
            if (count < config.Ai.DesiredGatherers && producer != null && money >= config.Role("eco.chinook").Cost && Send(new MatchOrder(Slot, OrderKind.Queue, new[] { producer.Id }, ProductId: "eco.chinook"))) money -= config.Role("eco.chinook").Cost;
            var reserve = missing == null ? 0 : config.Role(missing).Cost;
            // job.compose: each producer follows its portion of the configured composition, never a free unit.
            foreach (var building in own.Where(e => e.Completed && e.Queue.Length == 0 && e.RoleId is "prod.barracks" or "prod.factory"))
            {
                var products = config.Ai.Composition.Select(config.Role).Where(r => r.ProducerId == building.RoleId).ToArray();
                if (products.Length == 0) continue;
                var index = ProductionCursor.GetValueOrDefault(building.RoleId) % products.Length;
                var role = products[index];
                if (money - reserve < role.Cost) continue;
                if (Send(new MatchOrder(Slot, OrderKind.Queue, new[] { building.Id }, ProductId: role.Id)))
                { money -= role.Cost; ProductionCursor[building.RoleId] = (index + 1) % products.Length; }
            }
            var army = own.Where(e => e.Completed && e.ContainerId == 0 && !config.Role(e.RoleId).IsBuilding && config.Role(e.RoleId).Damage > 0).ToArray();
            SentArmy.RemoveWhere(id => !army.Any(e => e.Id == id));
            bool EnemySlot(int other) => other >= 0 && other != Slot && !(setup.Slots.Single(s => s.Index == Slot).Team > 0 && setup.Slots.Single(s => s.Index == Slot).Team == setup.Slots.Single(s => s.Index == other).Team);
            var assets = own.Where(e => e.RoleId is "prod.command" or "eco.dropoff").ToArray();
            var threat = view.Entities.Where(e => EnemySlot(e.OwnerSlot) && !e.IsRemembered && assets.Any(a => Distance2(a.Position, e.Position) <= (long)config.Ai.DefendRadius * config.Ai.DefendRadius)).OrderBy(e => Distance2(start, e.Position)).FirstOrDefault();
            if (threat != null)
            {
                foreach (var unit in army.Where(e => e.TargetId != threat.Id)) Send(new MatchOrder(Slot, OrderKind.AttackMove, new[] { unit.Id }, Position: threat.Position));
                return;
            }
            // job.attack: known enemy starts are static map information; mobile targets always come from LOS.
            var idle = army.Where(e => !SentArmy.Contains(e.Id) || e.Activity == EntityActivity.Idle).ToArray();
            if (idle.Length == 0 || idle.Length < config.Ai.AttackGroupSize && match.Tick - LastAttack < config.Ai.AttackDelayTicks) return;
            var knownBuilding = view.Entities.Where(e => EnemySlot(e.OwnerSlot) && config.Role(e.RoleId).IsBuilding).OrderBy(e => Distance2(start, e.Position)).ThenBy(e => e.Id).FirstOrDefault();
            WorldPoint destination;
            if (knownBuilding != null) destination = knownBuilding.Position;
            else
            {
                var starts = config.Map.Starts.Where(s => EnemySlot(s.Slot) && setup.Slots.Any(p => p.Index == s.Slot && p.Occupant is Occupant.Player or Occupant.AI) && !view.EliminatedSlots.Contains(s.Slot)).ToArray();
                if (starts.Length == 0) return;
                destination = starts[EnemyStartIndex % starts.Length].Command;
                if (Cell(view, config, destination) == Visibility.Visible) EnemyStartIndex++;
            }
            foreach (var unit in idle)
                if (Send(new MatchOrder(Slot, OrderKind.AttackMove, new[] { unit.Id }, Position: destination))) SentArmy.Add(unit.Id);
            LastAttack = match.Tick;
        }
        private static Visibility Cell(MatchSnapshot snapshot, GameConfig config, WorldPoint p) => snapshot.Visibility[p.Z / config.Map.CellSize * config.Map.WidthCells + p.X / config.Map.CellSize];
        private static string? MissingBuilding(GameConfig config, EntitySnapshot[] own)
        {
            var desired = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var role in config.Ai.BuildOrder)
            {
                desired[role] = desired.GetValueOrDefault(role) + 1;
                if (own.Count(e => e.RoleId == role) < desired[role]) return role;
            }
            return null;
        }
        private WorldPoint? Placement(IMatch match, GameConfig config, int dozer, string role, WorldPoint anchor)
        {
            int spacing = config.Ai.BuildSpacing;
            for (int ring = 1; ring <= config.Ai.PlacementSearchCells; ring++)
                foreach (var direction in Directions)
                {
                    var p = new WorldPoint(anchor.X + direction.X * ring * spacing, anchor.Z + direction.Z * ring * spacing);
                    if (match.CanPlace(Slot, dozer, role, p).Allowed) return p;
                }
            return null;
        }
    }
}
