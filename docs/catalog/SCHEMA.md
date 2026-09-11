# Schema

How the catalog is keyed. Stable ids are the API. Sheets join on them.

Do not invent a prefix. If a thing has no row, it is not in the game.

---

## How sheets join

```
FACTIONS  loadout id ─────────────► ROLES loadout grid
MECHANICS system index ──────────► one sheet per system
ROLES     id ─┬─ arm, dmg, del ──► DAMAGE, DELIVERY (combat join in ROLES)
              ├─ owner ──────────► ABILITIES
              ├─ from / needs ───► TECH_TREE
              ├─ stealth / det ──► DETECTION
              └─ leftover ───────► STATUSES (st / field / ent / clk)
MATCH_AI  slots, set.*, job.* ───► TEAMS, MAPS
MAPS      maps.* + layout.* ─────► MAP_GRAMMAR, TERRAIN, map.*
SIGHT     fog / radar ───────────► DETECTION (stealth only)
HUD / VOICE  ui, hud, vo ids ────► events, not copy
SLICE     filter ────────────────► intersection of the above for the first match
```

**Owner of a row:** the sheet whose table defines the id. Other sheets may *reference* it. If two sheets disagree on a row, the owner wins; then fix the other sheet.

| Kind | Owner |
| --- | --- |
| Unit, building, upgrade, power, map object, loadout | [ROLES.md](ROLES.md) |
| Unit/building special | [ABILITIES.md](ABILITIES.md) |
| Flag, field, leftover, clock | [STATUSES.md](STATUSES.md) |
| Damage kind, armor class | [DAMAGE.md](DAMAGE.md) |
| How a hit travels | [DELIVERY.md](DELIVERY.md) |
| Stealth kind, detector kind | [DETECTION.md](DETECTION.md) |
| Fog, shroud, LOS, radar | [SIGHT.md](SIGHT.md) |
| Path cell, logical tag | [TERRAIN.md](TERRAIN.md) |
| Prereq graph | [TECH_TREE.md](TECH_TREE.md) |
| Ally / FF rules | [TEAMS.md](TEAMS.md) |
| Map recipe | [MAP_GRAMMAR.md](MAP_GRAMMAR.md) |
| Map roster | [MAPS.md](MAPS.md) |
| Doctrine | [FACTIONS.md](FACTIONS.md) |
| Never-invent | [INVARIANTS.md](INVARIANTS.md) |

---

## Prefixes

| prefix | owner | meaning |
| --- | --- | --- |
| `ab.` | ABILITIES | Unit or building special |
| `addon.` | ABILITIES | Exactly-one mount |
| `aegis.` `forge.` `veil.` | ROLES / FACTIONS | Loadout |
| `ai.` | MATCH_AI | Difficulty |
| `air.` | ROLES | Aircraft |
| `arm.` | DAMAGE | Armor class |
| `armor.` | ROLES | Tank role |
| `arty.` | ROLES | Artillery |
| `boss.` | ROLES | AI-only loadout |
| `build.` | ROLES | Builder |
| `cam.` | HUD | Camera |
| `ch.` | MATCH_AI | Challenge (later) |
| `clk.` | STATUSES | Player-level clock |
| `deceive.` | ROLES | Deception unit |
| `def.` | ROLES | Defense building |
| `del.` | DELIVERY | How a hit travels |
| `det.` | DETECTION | Detector kind |
| `dmg.` | DAMAGE | Damage kind |
| `eco.` | ROLES | Economy unit or building |
| `ent.` | STATUSES | Leftover map entity |
| `field.` | STATUSES | Linger area |
| `fx.` | HUD | Feedback, not a screen |
| `hero.` | ROLES | Hero (one at a time) |
| `hud.` | HUD | In-match chrome |
| `inf.` | ROLES | Infantry |
| `job.` | MATCH_AI | AI job |
| `layout.` | MAP_GRAMMAR | Map recipe |
| `map.` | ROLES | Neutral map object |
| `maps.` | MAPS | Skirmish map |
| `mode.` | MATCH_AI | Mode |
| `pow.` | ROLES | Commander power |
| `power.` | ROLES | Power plant (or Veil's lack of one) |
| `prod.` | ROLES | Production / command / tech |
| `set.` | MATCH_AI | Match setting |
| `sfx.` | VOICE | UI tick |
| `sight.` | SIGHT | Shroud, fog, LOS, radar |
| `st.` | STATUSES | Flag on a body |
| `stealth.` | DETECTION | Hidden kind |
| `sw.` | ROLES | Superweapon building |
| `tag.` | TERRAIN | Logical tag (not paint) |
| `ter.` | TERRAIN | Path cell |
| `ui.` | HUD | Out-of-match screen |
| `up.` | ROLES | Research |
| `veh.` | ROLES | Vehicle |
| `vo.` | VOICE | Announcer event |
| `win.` | HUD | Modal / slide-out |

Role tables also use **side** values: `Aegis` `Forge` `Veil` `All` `Map`, or a loadout id.

---

## Combat join (columns)

Every shootable or shooting match object has a row in [ROLES.md](ROLES.md) **Combat join**.

| column | meaning |
| --- | --- |
| `id` | Role id |
| `arm` | Hull class. Occupied garrison uses `arm.garrison` as a *state* on top of the hull. |
| `dmg` | Primary outgoing kind. `—` if it does not shoot. |
| `del` | Primary [DELIVERY.md](DELIVERY.md) kind. `—` if it does not shoot. |

A second weapon lives in **notes** on the role, not a second id. Upgrades that *add* a weapon (`up.tow`) stay upgrades.

Example:

| id | arm | dmg | del |
| --- | --- | --- | --- |
| `inf.rocket` | `arm.infantry` | `dmg.rocket` | `del.missile` |
| `eco.dropoff` | `arm.structure` | — | — |

---

## New id

1. Pick an existing prefix.
2. Add the row on the **owner** sheet.
3. Add joins (combat, tech, ability owner, slice filter) in the same change.
4. Do not use the analog as the id.

---

## Out of schema

Numbers. Product names. Drawn geometry. Stack types. Announcer copy.
