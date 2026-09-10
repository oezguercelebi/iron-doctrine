# Map grammar

How maps are *made*. The roster of maps is [MAPS.md](MAPS.md). Objects from [ROLES.md](ROLES.md). Tags from [TERRAIN.md](TERRAIN.md).

This is **not** a 1v1 game. Source custom games go to **8 players**. Maps declare a max slot count. Skirmish fills those slots with people or AI.

---

## Hard rules

| rule | value |
| --- | --- |
| Max players on a map | 2–8 (map property) |
| Max in a match | 8 |
| Teams | FFA, or teams of 2–4 (2v2, 3v3, 4v4, 2v2v2v2, 1v3, 2v6, …) |
| Occupants | Player, AI, closed |
| You vs many AI | Legal. That is skirmish. |
| Goal | Multiplayer, up to 8, P2P / LAN, no backend |
| First step | 1 human vs 1 computer on a 2-slot map. Same sim. |

A map that only works as 1v1 is one map. It is not the map system.

---

## What every map must have

| piece | why |
| --- | --- |
| Start spots = max players | One command + builder per occupied slot |
| Close dock **per start** | First economy. Do not make two starts share the only dock. |
| Contest docks | Expand when close docks empty. Count scales with players (see recipes). |
| Garrison on docks and chokes | Infantry matter |
| Flat build pad at each start | Command + power (if any) + drop-off + barracks + factory without sitting on the dock |
| Unbuildable clutter | Stops a turret wall across the map |
| Fog | Intel |

Optional: oil, hospital, ZH pads, extra piles, cliffs, water, bridges.

---

## Slot counts

Ship maps in these bands. Odd counts (3, 5, 7) exist in the source community; we do not need them first.

| max | typical play | size band |
| --- | --- | --- |
| 2 | 1v1, 1v1 AI | small |
| 4 | 2v2, FFA 4, 1v3 | medium |
| 6 | 3v3, FFA 6, 2v4 | large |
| 8 | 4v4, FFA 8, 2v2v2v2, 1v7 | large / huge |

Size is relative. 8-player maps are bigger and have more contest money. They are not “the same 1v1 map with extra spawns glued on.”

---

## Layout recipes

### 2 slots — duel (`layout.duel`)

```
[A] close-dock  choke  contest  choke  close-dock [B]
```

Mirror or near-mirror. 2 close docks + 1–2 contest.

### 4 slots — corners (`layout.corners`)

Starts at four corners. Close dock each.

- **2v2:** allies on the same edge (A+B vs C+D) or diagonal (call it on the setup screen; default **adjacent** allies).
- **FFA:** four-way.
- Contest: centre town or two side docks + one centre tech.

```
[A] -------- [B]
 |  contest   |
[C] -------- [D]
```

### 4 slots — cross (`layout.cross`)

Starts at N S E W. Same play as corners, shorter centre walk.

### 6 slots — ring (`layout.ring6`)

Six starts around a centre. 3v3 (every other, or three adjacent) or FFA.

Close dock each. Centre = contest docks + tech. Do not starve the far side of money.

### 8 slots — ring / grid (`layout.ring8`, `layout.grid`)

Eight starts. 4v4 (two sides of a river/road), FFA, or 2v2v2v2.

Close dock each. Several contest docks, not one pit everyone suicides into. Two-axis chokes (a road and a town) so 4v4 is not a single deathball lane.

### Island (`layout.island`)

Water between starts. Ground path long or missing. Aegis air / Chinook matter. Veil hates this unless tunnels or boats (we have no navy — so island maps need *a* ground path or they ban Veil). **Every island map still has a ground route**, even if it is long.

### Urban (`layout.urban`)

Dense `map.garrison`. Short sight. Snipers and clear-tools matter. Works at any slot count.

### Choke (`layout.choke`)

Mountain / canyon. Few attack paths. Artillery and garrison dominate. Dangerous as 8p FFA; fine as 2p or 2v2.

### Money (`layout.money`)

Extra docks and oil. Long games, superweapons more likely. Needs more build space so turtle is possible.

---

## Play formats (setup, not map files)

The map only gives **slots**. Teams are a match setting.

| format | slots needed | notes |
| --- | --- | --- |
| 1v1 | 2 | Human vs human or vs 1 AI |
| 1vN | 2–8 | Skirmish vs computers. Fill remaining slots with AI |
| 2v2 | 4 | Shared contest, don’t steal ally close dock |
| 3v3 | 6 | |
| 4v4 | 8 | |
| FFA | 2–8 | Every slot its own team |
| 2v2v2v2 | 8 | Four teams |
| 2v6 / 1v7 | 8 | Human(s) vs a pack of AI |

AI must path, expand, and pick a target on **all** of these. `job.scout` looks at every enemy start, not “the other one.”

---

## Scaling (docks, tech, space)

| max players | close docks | contest docks (min) | capturable tech | build pad |
| --- | --- | --- | --- | --- |
| 2 | 2 | 1 | 0–2 | small |
| 4 | 4 | 2 | 2–6 | medium |
| 6 | 6 | 3 | 4–8 | large |
| 8 | 8 | 4 | 6–10 | large |

Tech (oil, hospital, pads) belongs in **contest** space, not inside a start pad.

---

## Design rules (all sizes)

- Two starts must never be forced to share a close dock.
- A dock without garrison or a choke is a rush parking lot.
- Superweapon footprints stay snipeable. No hiding them in unpathable holes.
- Allies need a path to each other that is shorter than the path to the enemy, or 2v2 is FFA with extra steps.
- 8-player maps need more than one front. If everything collapses to one tile, it is a 2-player map with spectators.
- Air (Aegis) wants at least one cliff or mountain the Chinook can cross. Not required on every map.
- Veil wants `tag.tunnel_legal` in dead ground. Not required on every map. Banned on maps with no ground route (see island).
- Closed slots do not spawn a dock owner. Unused close docks become extra contest money — **or** we hide unused starts. Pick one rule and keep it: **unused starts are closed: no spawn, their close dock stays as neutral contest.**

---

## First step vs the goal

First playable: **one** 2-slot map, 1 vs computer.

The catalog still describes 2–8 slots so we do not paint ourselves into a duel-only engine. Do not *build* 4/6/8 maps or a lobby before 1 vs computer is fun. Do not hard-code “only two players exist” in the sim.

---

## Not in this file

Campaign scripts. A map editor as a product. Copying official EA map names or layouts 1:1.
