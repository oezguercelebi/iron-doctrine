# Maps

Skirmish roster for the **whole app**. Original maps. Not EA names.

Player count is a property of the map. Fill slots with humans or AI. Max **8**.

Working titles. Change before ship.

---

## Set

| id | title | max | layouts | biome | play |
| --- | --- | --- | --- | --- | --- |
| `maps.two_ridge` | Two Ridge | 2 | `layout.duel` `layout.choke` | highland | 1v1 |
| `maps.two_yard` | Freight Yard | 2 | `layout.duel` `layout.urban` | industrial | 1v1 |
| `maps.two_flats` | Open Flats | 2 | `layout.duel` | scrub | 1v1, teaching |
| `maps.four_corners` | Four Corners | 4 | `layout.corners` | desert | 2v2, FFA, 1v3 |
| `maps.four_cross` | Crossroads | 4 | `layout.cross` `layout.urban` | city | 2v2, FFA |
| `maps.four_basin` | Dry Basin | 4 | `layout.corners` `layout.money` | desert | 2v2 long game |
| `maps.six_ring` | White Ring | 6 | `layout.ring6` | snow | 3v3, FFA, 2v4 |
| `maps.six_pass` | Ice Pass | 6 | `layout.choke` | snow / canyon | 3v3 |
| `maps.eight_plateau` | Plateau | 8 | `layout.grid` | desert plateau | 4v4, FFA, 1v7 |
| `maps.eight_coast` | Split Coast | 8 | `layout.ring8` `layout.island` | coast (ground route exists) | 4v4, 2v2v2v2 |
| `maps.eight_city` | Grid City | 8 | `layout.urban` `layout.grid` | city | 4v4, FFA |

Eleven maps. That is the **multiplayer app** set.

First step: build `maps.two_flats` for 1 vs computer. The rest wait.

---

## Contents (must match grammar)

Every row: max starts, that many close docks, contest docks and tech from the scaling table in [MAP_GRAMMAR.md](MAP_GRAMMAR.md).

| id | close docks | contest docks | extra |
| --- | --- | --- | --- |
| `maps.two_ridge` | 2 | 1 | cliffs, garrison on the pass |
| `maps.two_yard` | 2 | 1 | dense houses, one hospital |
| `maps.two_flats` | 2 | 1 | little clutter, slice-1 |
| `maps.four_corners` | 4 | 2 | centre oil |
| `maps.four_cross` | 4 | 2 | centre town, hospital |
| `maps.four_basin` | 4 | 3 | extra oil, repair pad |
| `maps.six_ring` | 6 | 3 | centre tech cluster |
| `maps.six_pass` | 6 | 2 | two passes, nasty chokes |
| `maps.eight_plateau` | 8 | 4 | two oil, one refinery |
| `maps.eight_coast` | 8 | 4 | water between some starts, **ground path around** |
| `maps.eight_city` | 8 | 4 | garrison everywhere, hospital |

---

## Skirmish filter

Setup screen lists maps whose `max >= occupied slots`.

- 2 humans + 0 AI → 2-slot maps (4/6/8 still legal if they want space).
- 1 human + 3 AI → need a 4+ map.
- 8 filled → only 8-slot maps.

Default pick: smallest map that fits the slot count.

---

## Challenge maps (later)

Challenge uses **uneven** maps (enemy base already up). Not this skirmish set. Do not reuse `maps.two_flats` as a Challenge home without a pre-built layout.

---

## Out of this catalog

- Exact dimensions, heightmaps, prop lists
- Official Generals / Zero Hour map names
- User map workshop (later, if ever)
