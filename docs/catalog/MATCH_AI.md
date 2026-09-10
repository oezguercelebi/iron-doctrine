# Match settings and AI

First product is local vs computer. This is that catalog.

---

## Modes

| id | analog | when |
| --- | --- | --- |
| `mode.skirmish` | Skirmish | 1–8 slots. Humans and AI mixed. First *shipped* match may hardwire 1v1; the mode is not 1v1-only |
| `mode.challenge` | Generals Challenge | Later. Named AI, pre-built base, sequential |
| `mode.lan` | LAN / custom | Later. Same rules as skirmish. No backend |

No campaign in the first several slices. No ranked.

---

## Skirmish slots

Up to 8.

| field | values |
| --- | --- |
| Occupant | Player / AI / Open / Closed |
| Team | 1–4 or FFA |
| Loadout | 12 commanders (slice-1: `aegis.vanilla` only) |
| Color | Distinct per slot |

---

## Match settings

| id | analog | default for first playable | notes |
| --- | --- | --- | --- |
| `set.map` | Map | Filter by slot count; default smallest that fits | See MAPS.md. 2 / 4 / 6 / 8 |
| `set.cash` | Starting credits | Medium | Low / med / high later |
| `set.start_units` | Dozer only vs extra | Command + dozer | Source could spawn extra |
| `set.crates` | Random money crates | Off | |
| `set.superweapons` | Allowed | Off in slice-1 | On/off |
| `set.fog` | Fog of war | On | |
| `set.speed` | Game speed | Normal | Local only |
| `set.limit_sw` | One super per player | n/a until supers | Optional later |

Win: all enemy **buildings** gone. Resign is a loss.

---

## Difficulty

| id | analog | computer may | computer may not |
| --- | --- | --- | --- |
| `ai.easy` | Easy | Play the same rules, slowly, bad army comp | Extra cash, map hack |
| `ai.medium` | Medium | Decent build order, expands, attacks | Extra cash, map hack |
| `ai.hard` | Hard | Tight build, uses counters, uses powers | Map hack. Slight cash trickle is a later discussion — default **no** |
| `ai.brutal` | Brutal | Hard + faster spend | Still no fog cheat unless we later say so |

Default: **the computer gets the same information and money rules as the player.** Difficulty is script quality, not god mode. If Brutal needs a cheat to be fun, log it as a setting, do not hide it.

---

## AI jobs (every difficulty)

| id | does |
| --- | --- |
| `job.base` | Plant command, power (if any), drop-off on the close dock, barracks, factory |
| `job.gather` | Keep gatherers on the dock. Add more. Expand when the dock dies |
| `job.scout` | Look at every enemy start and contest docks |
| `job.compose` | Mix rifle / rocket / tank / AA. Do not spam one role forever |
| `job.attack` | Attack-move a group at the enemy when it has a counter |
| `job.defend` | Pull back to the base if the command or drop-off is hit |
| `job.rebuild` | Replace lost production. Veil: crush is required, so the AI must retake holes |
| `job.power` | Later: spend promotion points. Use a click-power when it is free and useful |
| `job.super` | Later: build it, fire at production |

Slice-1 AI: `job.base` `job.gather` `job.compose` `job.attack` `job.defend`. No powers.

---

## Challenge (later)

| id | analog | notes |
| --- | --- | --- |
| `ch.pick` | Choose your general | One of 9 specialists (or vanilla) |
| `ch.list` | Sequential AI generals | Skip infantry and demo as *opponents* if we copy source; or don’t |
| `ch.home` | Their map, base already up | Uneven start. Win = destroy buildings |
| `ch.boss` | `boss.mix` | Mixed toys. Last fight |

Each challenge AI is a **personality** on a loadout (air rushes, turtle-beam, tank ball, stealth harass). Not a new faction.

---

## Slice-1 match

Hardwired: you `aegis.vanilla` vs `ai.medium` `aegis.vanilla` on `maps.two_flats`. Superweapons off, crates off. No 8-slot screen yet.

The **sim** still has slots (2 filled, rest closed). Multiplayer is the goal; this step is 1 vs computer. Do not write a separate “AI-only” match type.
