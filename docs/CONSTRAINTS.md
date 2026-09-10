# Constraints

Decided now. Stack is not among them.

## No backend

The game is a local program.

| Now | Later multiplayer |
| --- | --- |
| One process. Player + AI. | Players connect to each other. |
| No login. | No accounts. |
| No cloud. | No dedicated game server we operate. |
| Saves on disk if we add them. | Host or peer session. Disconnect ends the match. |

Classic RTS netcode for this shape is **deterministic lockstep** over LAN or peer-to-peer (the Age of Empires / original C&C pattern): every machine runs the same sim; only orders go over the wire. That is a later design, not a now decision.

What "no backend" forbids:

- Auth, profiles, friends lists as a service
- Ranked matchmaking as a service
- A simulation server
- Analytics or live-ops as a requirement to play

A later optional relay for NAT punch-through is a product question. Default is: it works on the same LAN without us.

## Original IP

Research docs name USA, China, GLA, and Zero Hour generals because that is the source. The game does not.

Do not ship:

- Command & Conquer, Generals, Zero Hour, SAGE, or EA marks
- Their unit, building, map, or character names
- Their art, audio, INI/XML data, or maps
- A 1:1 campaign recreation

Do ship:

- New faction names (working: Aegis, Forge, Veil)
- New commander names
- New unit names that describe the role, not the source unit
- New maps

The 2025 EA GPL source drop does **not** grant trademark or asset rights. We are not "Zero Hour, but ours."

## Tech stack: not chosen

Candidates, none preferred yet:

- Godot 4
- Unity
- Unreal
- Bevy / custom Rust
- A custom engine on wgpu / SDL

Pick when the first playable is specified, not before. The sim (economy, build, combat, AI) should stay separable from the renderer so the stack can move.

## Local vs computer first

Until that match is fun, we do not design netcode, a lobby UI, or a campaign.

## Content order

1. Shared systems (this research).
2. Per-unit, per-building, per-map inventory (next research).
3. Stack + first playable.
4. Other factions and commanders.
5. Peer multiplayer.

## Tone constraint

The 2003 GLA presentation aged badly (see source notes). Veil copies the **systems** (no power, holes, salvage, stealth, cheap spam) and not the **caricature**.
