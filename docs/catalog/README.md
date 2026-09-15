# Catalog

Role sheets for the whole game. No balance numbers. No frozen product names. The slice-1 Godot client already exists; this catalog is still the spec, not a second game.

Agent entry: [SLICE.md](SLICE.md) → [SCHEMA.md](SCHEMA.md) → [INVARIANTS.md](INVARIANTS.md) → the sheet you are building. Root: [AGENTS.md](../../AGENTS.md).

| File | What |
| --- | --- |
| [SCHEMA.md](SCHEMA.md) | Prefixes, joins, who owns an id |
| [INVARIANTS.md](INVARIANTS.md) | Locked rules. Never invent |
| [FACTIONS.md](FACTIONS.md) | Aegis / Forge / Veil + 12 loadouts |
| [MECHANICS.md](MECHANICS.md) | Every system, grouped. Index of the sim |
| [ROLES.md](ROLES.md) | Every unit, building, power, upgrade, map object + combat join |
| [ABILITIES.md](ABILITIES.md) | Unit/building specials, toggles, add-ons |
| [STATUSES.md](STATUSES.md) | Flags, linger fields, leftover entities (hole, salvage, mines) |
| [TECH_TREE.md](TECH_TREE.md) | Prerequisites. What unlocks what |
| [DETECTION.md](DETECTION.md) | Stealth kinds vs detectors |
| [SIGHT.md](SIGHT.md) | Shroud, fog, LOS, radar |
| [DAMAGE.md](DAMAGE.md) | Damage kinds vs armor classes |
| [DELIVERY.md](DELIVERY.md) | How a hit travels |
| [TERRAIN.md](TERRAIN.md) | Path tags and map layers |
| [TEAMS.md](TEAMS.md) | Allies, friendly fire, what is not shared |
| [HUD.md](HUD.md) | Screens, HUD, camera, command bar |
| [MATCH_AI.md](MATCH_AI.md) | Modes, settings, difficulty, AI jobs |
| [VOICE.md](VOICE.md) | Announcer events. Not the script |
| [MAP_GRAMMAR.md](MAP_GRAMMAR.md) | How maps are made. 2–8 players, layouts |
| [MAPS.md](MAPS.md) | Skirmish map roster (not 1v1-only) |
| [SLICE.md](SLICE.md) | First playable, as a filter |

## How to read a row

- **id** — stable key. Use this in data later. Do not ship it as a UI name.
- **role** — what it *is* in the match.
- **side** — `Aegis` / `Forge` / `Veil` / `All` / `Map` / a commander id.
- **analog** — source research only. Never a shipping name.
- **beats / beaten by** — the counter, not DPS.
- **arm / dmg / del** — combat join on ROLES.md. Armor class, damage kind, delivery.
- **notes** — special rules. Empty means the group mechanic is enough.

Specialist commanders **replace or drop** a vanilla role. They do not invent a thirteenth faction. See the loadout grid at the end of ROLES.md and [FACTIONS.md](FACTIONS.md).

If catalog and research disagree, catalog wins.

## Out of this catalog

- HP, cost, build time, range, XP thresholds
- Product names for units
- Heightmaps and prop lists (map *ids* are in MAPS.md)
- Stack, renderer, netcode
- Campaign missions
- Announcer *copy* (events are listed; lines are not)
