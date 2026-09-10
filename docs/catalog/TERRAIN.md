# Terrain tags

What a map cell is. Objects (`map.*`) sit on top.

---

## Path tags

| id | ground | air | build | notes |
| --- | --- | --- | --- | --- |
| `ter.ground` | yes | yes | yes | Default |
| `ter.road` | yes (faster optional) | yes | yes | Cosmetic unless we add a speed buff |
| `ter.cliff` | no | yes | no | Saboteur may climb. Chinook ignores |
| `ter.slope` | yes if not too steep | yes | no | |
| `ter.water` | no | yes | no | No navy in this game |
| `ter.bridge` | yes | yes | no | Destructible later; slice-1 can be static |
| `ter.shore` | yes | yes | no | |
| `ter.unbuildable` | yes | yes | no | Decor, rocks, mission blockers |
| `ter.start` | yes | yes | yes | Spawn. Command footprint |
| `ter.block` | no | no | no | World edge, mountains that even air treats as out of map |

Air ignores ground path except `ter.block`.

---

## Logical tags (not paint)

| id | meaning |
| --- | --- |
| `tag.garrisonable` | Civilian box. See `map.garrison` |
| `tag.dock` | Supply. See `map.dock` |
| `tag.pile` | Small supply |
| `tag.tech` | Capturable neutral |
| `tag.choke` | Designers: this is where garrison and artillery matter |
| `tag.tunnel_legal` | Veil may place a tunnel / sneak entrance |
| `tag.camera_ok` | Camera may go here |

---

## Layers

1. Height / path (this file)
2. Objects (`map.*`, buildings, units)
3. Fields (`field.*`)
4. Fog / shroud

Pathfinding: ground grid or navmesh. Air: straight line with `ter.block` only. Tunnels: graph of nodes, not terrain.

---

## Slice-1

`ter.ground`, `ter.unbuildable`, `ter.start`, `tag.garrisonable`, `tag.dock` on a **2-slot** map.

The tag set above is for every map size. Water / cliffs / bridges come with the 4–8 slot maps that need them. Do not invent a second terrain system for “big maps.”
