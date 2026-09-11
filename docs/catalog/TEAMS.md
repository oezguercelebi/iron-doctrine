# Teams

Slots and what “ally” means. Occupants and settings live in [MATCH_AI.md](MATCH_AI.md). Vision sharing lives in [SIGHT.md](SIGHT.md).

The map only gives **slots**. Teams are a match setting.

---

## Slot

Up to 8.

| field | values |
| --- | --- |
| Occupant | Player / AI / Open / Closed |
| Team | `1`–`4` or FFA (each slot its own team) |
| Loadout | 12 commanders |
| Color | Distinct per slot |

Closed: no spawn. That start’s close dock stays contest ([INVARIANTS.md](INVARIANTS.md)).

---

## Ally vs enemy

**Ally** = same team number, not FFA.

| action | ally | enemy | note |
| --- | --- | --- | --- |
| Shared shroud / fog / LOS / radar | yes | no | `sight.shared` |
| See HP / vet on their units | yes | only in LOS as usual | No command of them |
| Select, order, control group | no | no (theirs) | No shared control |
| Spend their cash | no | no | Cash is per player |
| Use their drop-off | no | no | Gatherers return to *you* |
| Queue in their production | no | no | |
| Repair their buildings (`ab.repair`) | yes | no | Builders only |
| Sell their buildings | no | no | |
| Enter their tunnels | no | no | Owner only, even allied Veil |
| Occupy their owned garrison | no | combat-drop / clear | Civilian empty = free; once occupied it is that player’s |
| Capture their building | no | yes, channel | |
| Auto-acquire with guns | no | yes | |
| Force-attack | yes (FF) | yes | |
| Commander XP from their kills | no | n/a | Your kills, your bar |
| Win together | yes | no | Team wins when *all other teams* have no buildings |

FFA: every slot is an enemy. 2v2v2v2 uses four team numbers.

---

## Friendly fire (locked)

| kind | hits allies / self? |
| --- | --- |
| Auto-acquire small-arms, cannon, rockets, sniper, laser | no |
| Splash, flame, toxin, rad, mines, traps, crush, explosives, superweapons | **yes** |
| Force-attack (`Force-attack` order) | **yes** |
| Microwave bubble, EMP pulse | yes (treat as splash/disable) |

Do not add a “disable FF” match setting in slice-1.

---

## Not shared (locked)

- Cash, queues, power pool, promotion points, hero cap.
- Drop-off, air pads, tunnel graph.
- Control of units.

Allied pathing: [MAP_GRAMMAR.md](MAP_GRAMMAR.md) — allies need a shorter path to each other than to the enemy, or 2v2 is FFA with extra steps.

---

## Slice-1

Two slots, two teams (you vs computer). No allies. FF rules still apply to self-splash (you can nuke your own tanks later; in slice-1, crush and explosives are limited). Sim still has a team field per slot.
