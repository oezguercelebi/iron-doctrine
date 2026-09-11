# HUD and screens

What the player sees. No art. No layout pixels.

---

## Out of match

| id | analog | has | notes |
| --- | --- | --- | --- |
| `ui.menu` | Main menu | Skirmish, options, quit. Later: Challenge, LAN | No account, no store |
| `ui.skirmish` | Skirmish setup | Map (filtered by slot count), up to 8 slots, teams, AI, settings from MATCH_AI | 1v1 is a filling of 2 slots, not a different app |
| `ui.challenge` | Generals Challenge | Pick your loadout, fight a list | Later |
| `ui.options` | Options | Graphics, audio, speed, keybinds | |
| `ui.end` | Defeat / victory | Rematch, setup, menu | |

No login. No lobby server. LAN screen later, host/join on the LAN.

---

## In match — always on

| id | analog | shows |
| --- | --- | --- |
| `hud.money` | Credits | Cash |
| `hud.power` | Power meter | Aegis / Forge only. Current vs drain. Hidden for Veil |
| `hud.minimap` | Radar | Terrain + detected. Off if no radar / low power / no van. Rules: [SIGHT.md](SIGHT.md) |
| `hud.selection` | Portrait | HP, vet chevrons, passenger count, one addon slot |
| `hud.command` | Command bar | Context buttons for the selection (build, train, ability, sell) |
| `hud.generals` | Star button | Opens promotion window. Shows unspent points |
| `hud.clocks` | Superweapon / power portraits | Own charge. Enemy superweapon announced when *they* start the clock |
| `hud.idle` | Idle worker | Jump to idle builder / gatherer |
| `hud.menu` | Esc | Pause (local). Resign. Settings. |

---

## Windows (modal or slide-out)

| id | analog | does |
| --- | --- | --- |
| `win.generals` | Generals Window | XP bar, points, tree. Click to buy `pow.*` |
| `win.pause` | Pause | Local vs AI only |
| `win.place` | Build ghost | Footprint, power draw, invalid red |
| `win.sell` | Sell cursor | Click own building |

---

## Feedback (not a screen)

| id | analog | when |
| --- | --- | --- |
| `fx.select` | Decal | Selected |
| `fx.waypoint` | Path ticks | Move order |
| `fx.under_attack` | Minimap ping + VO | Own unit/building hit |
| `fx.promote` | VO + star | Rank up |
| `fx.sw_enemy` | Banner + VO | Enemy superweapon building started / fired |
| `fx.stealth_shimmer` | Heat haze | Detected stealth, or own stealth |
| `fx.hole` | Crater prop | `ent.hole` |
| `fx.low_power` | Meter flash + VO | Brownout |

---

## Camera

| id | analog | notes |
| --- | --- | --- |
| `cam.pan` | WASD / edge / middle-drag | |
| `cam.zoom` | Wheel | RTS pitch, not FPS |
| `cam.rotate` | Optional | Source allowed a little yaw |
| `cam.bookmark` | F-keys / control groups for camera | Source had bookmarks |
| `cam.follow` | Double-click unit | Optional |
| `cam.jump_event` | Space / idle / attack ping | |

---

## Command bar grammar

Selection decides the grid:

- Nothing → no train buttons (or default to command if selected)
- Building → train / research / rally / sell / building ability
- Unit → move-implied, stop, guard, abilities
- Mix → only shared verbs

Control groups 1–9. Double-tap jumps camera.

---

## Slice-1 HUD

`hud.money`, `hud.power`, `hud.minimap`, `hud.selection`, `hud.command`, `cam.pan`, `cam.zoom`, `win.place`. No generals window until promotion ships.
