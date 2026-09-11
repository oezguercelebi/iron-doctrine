# Roadmap

Phases 0–2 are complete. The accepted first playable is implemented and blocked in final review on two gameplay edge cases. Later phases are not scheduled.

## 0. This repo (done)

- Game name
- Source-game research
- Shared mechanics
- Goal and constraints
- Git repo

## 1. Mechanic inventory (done)

Catalog: [docs/catalog/](catalog/README.md).

- Schema, invariants, factions (our names)
- Systems, roles, abilities, statuses, tech tree
- Detection, sight (fog/radar), damage×armor, delivery
- Teams, HUD, match settings, AI jobs, announcer events
- Map grammar for 2–8 players; skirmish map roster
- 12 loadouts as deltas
- Slice-1 as one filter sheet; combat join on every role

Final balance and product names are still later. The slice uses one labeled placeholder tuning file and an original mirrored map. The map *roster* (2–8 players) is catalogued.

## 2. Stack decision (done)

Godot **4.8 .NET**, top-down 3D. Sim is a tick + orders, not `Node`. View is glTF. Later MP is lockstep LAN/P2P over ENet, no backend. Until 4.8 stable, 4.7.2 is allowed. See [CONSTRAINTS.md](CONSTRAINTS.md).

## 3. First playable — 1 vs computer

One human, one AI, one 2-slot map, vanilla Aegis. Win by destroying enemy buildings. Proof is a match you can finish, not a menu. No netcode. See [slice-1-playable](cases/slice-1-playable/status.md) and its [acceptance evidence](cases/slice-1-playable/acceptance.md).

## 4. Multiplayer (the goal)

Same match, two humans. LAN / peer-to-peer. No backend. Then more slots.

## 5. Rest of the roster

More maps (4 / 6 / 8). Forge, Veil. Specialist commanders. Challenge.

## Explicitly later or never

- Story campaign
- Map editor as a product
- Ranked service
- Mobile
