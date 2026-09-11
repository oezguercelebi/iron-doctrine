# Constraints

Decided now.

## No backend

The game is a local program.

| Now | Later multiplayer |
| --- | --- |
| One process. Player + AI. | Players connect to each other. |
| No login. | No accounts. |
| No cloud. | No dedicated game server we operate. |
| Saves on disk if we add them. | Host or peer session. Disconnect ends the match. |

Classic RTS netcode for this shape is **deterministic lockstep** over LAN or peer-to-peer (the Age of Empires / original C&C pattern): every machine runs the same sim; only orders go over the wire. Slice-1 still has no netcode; the shape is locked in **Tech stack** below.

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

## Tech stack

**Engine:** Godot **4.8 .NET** (C#). Top-down 3D. Desktop. MIT.

4.8 is the current line. Until 4.8 *stable* ships, slice-1 may start on **4.7.2** (latest stable as of 2026-09) and jump, or on a 4.8 snapshot if we accept pre-release. Do not start a 4.6 (or older) project. Do not treat 4.8-dev as production without saying so in the case.

**Sim:** one clock, order list, tick function. C# or GDExtension. **Not** `Node` as source of truth. No engine RNG as the match RNG. Slice-1 has no sockets; that same tick is what multiplayer will feed.

**View:** import **glTF from Blender** (or Meshy). Cubes are placeholders only. 4.8 texture streaming and Trail3D are in-bounds; do not promise Nanite/Lumen.

**Net (later, not slice-1):** deterministic lockstep over LAN/P2P. Only orders on the wire. ENet/UDP as a dumb pipe. **Not** Godot MultiplayerAPI / scene replication. Host or peer. Disconnect ends the match. LAN with no account.

Forbidden:

- Unreal (or any engine) as the *match* via actor replication / dedicated server
- PlayFab, Unity Gaming Services, Photon, EOS as a requirement to play
- Custom wgpu / SDL for slice-1
- Putting the match in the node tree so two machines cannot tick the same orders

## Local vs computer first

Until **1 vs computer** is a match you can finish and care about:

- No netcode
- No lobby UI
- No campaign
- No 8-player screen

Keep the sim as a match (slots, orders, one clock) so multiplayer is a connection, not a rewrite.

## Content order

1. Shared systems (research).
2. Catalog (`docs/catalog/`).
3. **1 vs computer** (vanilla Aegis, one 2-slot map). Stack is this file.
4. **Multiplayer** on that same match (2 humans, P2P / LAN, no backend).
5. More slots, maps, factions, commanders.

## Tone constraint

The 2003 GLA presentation aged badly (see source notes). Veil copies the **systems** (no power, holes, salvage, stealth, cheap spam) and not the **caricature**.
