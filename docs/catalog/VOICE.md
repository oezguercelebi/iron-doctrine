# Voice and announcer

Events that speak. Copy is original later. This is the **event list**, not the script.

Faction flavor: Aegis clipped professional. Forge industrial / mass. Veil irregular / radio. Do not copy EA lines.

---

## Announcer (player-facing system VO)

| id | analog | when |
| --- | --- | --- |
| `vo.funds` | Insufficient funds | Train / build / research without cash |
| `vo.power` | Low power | Cross into brownout |
| `vo.power_back` | Power restored | Optional |
| `vo.building_done` | Construction complete | Building finishes |
| `vo.unit_ready` | Unit ready | Train finishes |
| `vo.upgrade_done` | Upgrade complete | |
| `vo.busy` | On hold / queue full | Optional |
| `vo.under_attack` | Our base is under attack | Own building hit |
| `vo.unit_lost` | Unit lost | Optional, easy to spam — gate it |
| `vo.promote` | Promotion / rank | Commander rank up |
| `vo.sw_own_ready` | Superweapon ready | Own clock full |
| `vo.sw_own_fire` | Launched | Own fire |
| `vo.sw_enemy_build` | Detected | Enemy super building started |
| `vo.sw_enemy_fire` | Incoming | Enemy fired |
| `vo.victory` | Victory | |
| `vo.defeat` | Defeat | |

---

## Unit acknowledgements

Every trainable unit: **select**, **move**, **attack**, **ability**. Builders add **build**. Gatherers add **gather**.

Do not write the lines in this catalog. One bank per side is enough at slice-1 (shared rifle/tank barks).

---

## UI ticks (not voice)

| id | when |
| --- | --- |
| `sfx.click` | Command bar |
| `sfx.place` | Ghost placed |
| `sfx.invalid` | Bad place / bad order |
| `sfx.rally` | Rally set |
| `sfx.notify` | Ping |

---

## Slice-1

`vo.funds`, `vo.power`, `vo.building_done`, `vo.unit_ready`, `vo.under_attack`, `vo.victory`, `vo.defeat`. Placeholder beeps are fine.
