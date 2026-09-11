# Sight

Who sees the map. Stealth is [DETECTION.md](DETECTION.md). This sheet is ordinary intel: shroud, fog, LOS, radar.

Numbers (radii, pulse length) later.

---

## Layers

| id | analog | does |
| --- | --- | --- |
| `sight.shroud` | Unexplored black | Never seen. Camera is black. Minimap empty there. |
| `sight.fog` | Fog of war | Explored. Terrain last-seen. Units hidden unless in LOS (or a pulse). |
| `sight.los` | Unit / building vision | Reveals shroud, clears fog, shows unstealthed things in radius. |
| `sight.radar` | Minimap blips | Unit blips on the minimap. Needs a radar *source* and not `st.low_power`. |
| `sight.pulse` | Satellite, van scan, sat-hack | Temporary full reveal in a radius. Not standing detection. |
| `sight.shared` | Allied intel | Allies share shroud, fog, LOS, and radar. See [TEAMS.md](TEAMS.md). |
| `sight.height` | Cliff vision | **Not in slice-1.** Later: cliffs block ground LOS through the volume. Air ignores that. |

No tree / bush cover system. Do not add one.

Radar is **not** vision. Local camera still shows `sight.los` when radar is off.

---

## LOS

- Every unit and completed building has a vision radius (number later).
- Stealthed things in LOS still need a detector. [DETECTION.md](DETECTION.md).
- Incomplete buildings do not see (they are lost if the builder dies anyway).
- Dead units drop LOS immediately.
- Occupied `map.garrison` sees as a building; occupants do not add a second radius.

---

## Radar sources

No radar → **no unit blips** on the minimap. Explored terrain may still draw. Pings and selection still work.

| side | source | off when |
| --- | --- | --- |
| Aegis | `prod.command` innate | Command gone, or `st.low_power` |
| Forge | `ab.radar_upgrade` on `prod.command` | Not researched, command gone, or `st.low_power` |
| Veil | `veh.radar` alive | Van dead. Veil has no power brownout. |

`sight.pulse` (`ab.satellite`, `ab.van_scan`, `up.sat_hack`) fills shroud + fog + blips in the radius for a short clock. Pulse is a reveal, not a standing `det.*`.

---

## Camera vs intel

- The camera may pan into shroud. It does not reveal it.
- `tag.camera_ok` is where the camera may go. See [TERRAIN.md](TERRAIN.md).
- Minimap click jumps the camera. No blips in shroud.

---

## Slice-1

`sight.shroud`, `sight.fog`, `sight.los`. Aegis radar on `prod.command` (`sight.radar`). No stealth. No `sight.height`. No pulses (satellite is not in the slice-1 ability list). Two enemies: no `sight.shared` yet, but the flag still exists on the sim.
