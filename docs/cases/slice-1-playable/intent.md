# Accepted intent — 2026-09-11

The user explicitly lifts the scaffold ban and accepts building slice-1 now. Start a Godot 4.7.2 .NET client (4.8 stable later, no older engine and no unaccepted snapshot), top-down 3D, desktop, MIT. Proof is a local match that boots directly, can be played, and finishes in victory or defeat. User closes the case; no push authorized.

Catalog wins over research. Read AGENTS.md, SLICE, SCHEMA, INVARIANTS, then system sheets. No analog names/copy/assets. Stable ids remain data; working UI labels only. All placeholder gameplay numbers in ONE labeled data file with slice role ids only. Original glTF meshes authored via Blender (or Meshy), not cubes as final look.

## Match and architecture
Hardwired mode.skirmish, maps.two_flats/layout.duel, Player aegis.vanilla team 1 vs AI ai.medium aegis.vanilla team 2. Eight slot model: two filled, rest closed; occupant, team, loadout, color retained and full catalog prefix/loadout space preserved. No two-player assumptions in targeting/pathfinding. set.fog on, superweapons/crates off, start_units command + dozer, cash Medium placeholder. One clock/order list/tick, plain C# sim independent of Godot Node, deterministic own RNG. No sockets/backend or scene replication. Local pause. Enemy buildings eliminated = win; army wipe recoverable; resign = loss.

## Required role behavior
- build.dozer: one building at a time, legal forward construction, repairs own/ally buildings. Death/invalid placement cancels; dozer death loses incomplete building, no husk.
- eco.dropoff: first eco.chinook free on completion; trains more; gatherers deliver only to owner.
- eco.chinook (UI Gatherer): flying gatherer/infantry transport, fat placeholder capacity, ignores ground path; rockets/AA kill it, tank cannon cannot. No combat-drop.
- power.fusion: pool vs building drain. st.low_power when drain exceeds supply; radar and defenses off.
- prod.command: starting building, trains dozers, innate radar (not vision).
- prod.barracks: Rifle/Rocket production, up.capture research.
- prod.factory: requires dropoff, Tank/Scout production, repairs returning vehicles.
- inf.rifle: small arms, capture after research via interruptible channel.
- inf.rocket: missile, good vs tanks/air, poor vs infantry.
- armor.basic: cannon good vs vehicles/structures, none vs air; infantry crush, including friendly.
- veh.scout_gun: holds five infantry firing out; no TOW.
- def.patriot (UI AA turret): requires fusion, powered rocket defense vs air/vehicles; optional neighbor link can be skipped.
- map.dock: finite, indestructible, one gatherer loads at a time, extras wait.
- map.garrison: infantry enter/exit, occupied arm.garrison, occupants spill on destruction, no clear tools.
Start-available buildings: command, fusion, barracks, dropoff; factory requires dropoff and AA requires fusion. Start entities remain ONLY command + dozer per player.

## Systems and inputs
MECHANICS 1–11 and 14–15: match/orders/construction/economy/power/production/combat/garrison/veterancy/capture/fog/transport/repair. Select, box selection, control groups 1–9; move, attack, attack-move, stop, guard, waypoint, force-attack, build, rally, queue/cancel, sell, repair, gather, enter/exit, capture. Dozer ghost respects terrain/building collision. Queue costs/refunds per player. Capture flips buildings after channel, interruption possible. Sell refund placeholder.

Damage kinds small/cannon/rocket/crush and armor infantry/light/tank/air/structure/garrison. Matrix: small good/ok/poor/poor/poor/poor; cannon ok/good/good/none/good/poor; rocket poor/ok/good/good/good/poor; crush good/none/none/none/none/none. Instant/missile/melee/channel delivery only. Missiles in world. Auto guns/rockets avoid allies; force-attack/crush hit allies. Even poor fights must lose. Kill-value Veteran→Elite→Heroic improves damage, rate, health; Elite/Heroic self heal. Place XP table in number file.

Sight: shroud/fog/LOS, command radar not vision, radar off if command lost or low power. No height/pulses/shared sight in play. Teams retained, cash/queues/power/control separate.

## Map, UI, AI
Original mirrored scrub map, two generous flat build pads, two close docks + contest dock, garrisons at docks/chokes, unbuildable clutter preventing turret wall. No water/cliffs/bridges/tree cover. ter.ground/unbuildable/start, tag.garrisonable/dock. Unused-start close dock would stay neutral.
HUD money/power/minimap/selection/command, pan/zoom, build ghost. Pause/resign local menu; victory/defeat with rematch/quit. Beeps for funds/power/building_done/unit_ready/under_attack/victory/defeat. No lobby/login.
AI uses same orders, money and fog. job.base builds fusion/dropoff/barracks/factory from command/dozer; gather grows and expands on depletion; compose mixed rifle/rocket/tank/AA; attack-move counter group; defend pulls back when command/dropoff hit. No cheats/powers.

## Excluded
Forge/Veil/specialists/promotion/superweapons/stealth/linger/hole/salvage/full loadouts/4–8-slot maps/LAN/Challenge/campaign/navy/wall lines/construction yard/tree cover/instant capture/backend/ranked/login. No airfield/tech/drop pad/heroes, flashbang/flame/microwave/combat-drop/TOW or invented catalog rows. No production product names. No research analog names in displayed text.
