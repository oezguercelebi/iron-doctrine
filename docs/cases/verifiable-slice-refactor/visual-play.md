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

## Captures (to be filled after I-client / render)

| File | Scenario | Notes |
| --- | --- | --- |
| pending | locomotion infantry | controlled camera 1440×900 |
| pending | chinook rotors | |
| pending | instant tracer vs cooldown-only negative | |
| pending | HUD chrome vs world | |

Cross-GPU pixel identity is not the target. User accepts or overrules at close.
