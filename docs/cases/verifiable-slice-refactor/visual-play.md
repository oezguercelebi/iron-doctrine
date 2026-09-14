# Visual / play acceptance (user gate)

Engineering evidence is separate. This package is for the user’s close-time judgment. Not an automated APPROVE.

## What to look at

1. Infantry (`inf.rifle`, `inf.rocket`) moving: root follows sim; facing changes; slight bob — not ice-slide, not a cinematic walk cycle.
2. Vehicles (`armor.basic`, `veh.scout_gun`, `build.dozer`): same; no fake treads required.
3. Chinook: rotors spin while airborne/operating; stop when gone/contained.
4. Structures stay planted.
5. Instant fire: tracer only when a shot resolved. Missiles are world objects.
6. Fog: no enemy queue/cargo/activity as facts; remembered buildings stale.
7. HUD clicks do not issue world moves.

## Captures (this machine)

Platform: Darwin arm64, Godot 4.7.2 .NET, renderer `gl_compatibility`, OpenGL 4.1 Metal, Apple M1 Pro. Viewport 1440×900. Not a cross-GPU pixel gate.

| File | Scenario | Notes |
| --- | --- | --- |
| `proof/frames/build-ghost.png` | `client.input_build_ghost_point` | Dozer constructing Fusion after real HUD card + PushInput. display=macOS. |
| `proof/frames/pause.png` | `match.pause_freezes_clock` | Escape pause. |
| `proof/frames/hud-chrome.png` | `client.no_fog_world_leak` | Chrome RMB vs world Move. |

Not in these stills: infantry bob, chinook rotor spin, instant tracer vs cooldown. Those remain user feel plus client transform checks. Do not auto-approve regenerated goldens.

Cross-GPU pixel identity is not the target. User accepts or overrules at close.
