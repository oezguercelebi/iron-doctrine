# Catalog

Role sheets for the whole game. Still no app. Still no numbers. Still no frozen product names.

| File | What |
| --- | --- |
| [MECHANICS.md](MECHANICS.md) | Every system, grouped. This is what the sim must implement. |
| [ROLES.md](ROLES.md) | Every unit, building, power, upgrade, map object |
| [ABILITIES.md](ABILITIES.md) | Unit/building specials, toggles, add-ons |
| [STATUSES.md](STATUSES.md) | Flags, linger fields, leftover entities (hole, salvage, mines) |
| [TECH_TREE.md](TECH_TREE.md) | Prerequisites. What unlocks what |
| [DETECTION.md](DETECTION.md) | Stealth kinds vs detectors |
| [DAMAGE.md](DAMAGE.md) | Damage kinds vs armor classes |
| [TERRAIN.md](TERRAIN.md) | Path tags and map layers |
| [HUD.md](HUD.md) | Screens, HUD, camera, command bar |
| [MATCH_AI.md](MATCH_AI.md) | Modes, settings, difficulty, AI jobs |
| [VOICE.md](VOICE.md) | Announcer events. Not the script |
| [MAP_GRAMMAR.md](MAP_GRAMMAR.md) | How maps are made. 2–8 players, layouts |
| [MAPS.md](MAPS.md) | Skirmish map roster (not 1v1-only) |

## How to read a row

- **id** — stable key. Use this in data later. Do not ship it as a UI name.
- **role** — what it *is* in the match.
- **side** — `Aegis` / `Forge` / `Veil` / `All` / `Map` / a commander id.
- **analog** — source research only. Never a shipping name.
- **beats / beaten by** — the counter, not DPS.
- **notes** — special rules. Empty means the group mechanic is enough.

Specialist commanders **replace or drop** a vanilla role. They do not invent a thirteenth faction. See the loadout grid at the end of ROLES.md.

## Out of this catalog

- HP, cost, build time, range, XP thresholds
- Product names for units
- Heightmaps and prop lists (map *ids* are in MAPS.md)
- Stack, renderer, netcode
- Campaign missions
- Announcer *copy* (events are listed; lines are not)
