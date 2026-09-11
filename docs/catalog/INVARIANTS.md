# Invariants

Locked. An implementer who “fixes” one of these has left the game.

Catalog wins if research disagrees.

---

## Source of truth

- The catalog is the spec. Research is analog.
- If a system has no catalog row, it is **not in the game**. Do not invent it to match a memory of 2003.
- Slice-1 is a **filter** on this catalog, not a second ruleset. See [SLICE.md](SLICE.md).

## IP

- Do not ship Command & Conquer, Generals, Zero Hour, SAGE, or EA marks.
- Do not ship their unit, building, map, or character names.
- Analog columns are research only.
- Working names Aegis / Forge / Veil are placeholders. Loadout **ids** stay.

## Match shape

- The sim is a match: slots, orders, one clock. 1 vs computer is two filled slots, not a different mode.
- Max 8 players. Do not hard-code “only two exist.”
- **Win:** all *enemy* buildings gone (other teams). Army wipe is recoverable. Resign is a loss.
- Pause exists in local vs AI only.
- No backend. No accounts. No dedicated sim server.

## Construction and economy

- Buildings come from a **builder unit**, not a construction yard / MCV unpack.
- Place anywhere legal. Forward bases are legal.
- **Incomplete building is lost** if the builder dies. No husk. Locked.
- Unused starts: no spawn; their close dock stays as **neutral contest**. Locked. See [MAP_GRAMMAR.md](MAP_GRAMMAR.md).
- One gatherer loads a dock at a time.
- Gatherers return to *your* drop-off, never an ally’s.
- Docks (`map.dock`) are indestructible.

## Factions

- Three playable doctrines. Specialists are loadout deltas, not a fourth side.
- **Veil: no power grid.** Never add a Veil plant.
- **Veil: no air force.** Never add a Veil airfield or plane. AA is their air.
- **No navy.** Island maps still have a ground route.
- Aegis radar is on `prod.command` from the start. Forge radar is a command **research**. Veil radar is `veh.radar`.
- One hero at a time per player.
- Capture is a **channel**, not instant.
- Veil hole (`ent.hole`) must exist when Veil exists. Beam deletes holes. Crush deletes holes. Ignoring a hole rebuilds the building.

## Combat

- No StarCraft attack/defend/hold stances. Unpack/pack is only for roles that say so.
- Soft army cap is cash + build time. Engine may have a hard cap; that cap is not a designed command-point layer. Number later.
- One skirmish promotion tree per loadout. Do not import campaign trees.
- Friendly fire and sharing: [TEAMS.md](TEAMS.md). Do not silently share cash, queues, or tunnels.

## Catalog hygiene

- No HP, cost, time, range, or XP **numbers** in these sheets.
- No new prefix without [SCHEMA.md](SCHEMA.md).
- `up.horde` is **innate** (Forge rifle / rocket / basic tank at 5+). It is not a research you buy. Nationalism fattens it.
- `prod.detention` is optional leftover from vanilla Generals. Not in slice-1. Default: **drop** unless a later case revives intel-as-building.
- `power.none` is a faction rule, not a building you place.

## Never invent

| Temptation | Rule |
| --- | --- |
| Boats, hovercraft, naval yards | No |
| Walls as a buildable line | No |
| Tree / bush cover as a combat system | No |
| Instant engineer capture | No |
| Shared control of ally units | No |
| Ranked service, login, cloud save | No |
| Story campaign in the first several slices | No |
| A 1v1-only pathfinder or targeting system | No |
| A fourth playable faction | No |
| Product names copied from analog | No |
