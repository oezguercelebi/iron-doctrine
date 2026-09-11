# Iron Doctrine

A real-time strategy game of **named commanders and clashing doctrines**.

Inspired by *Command & Conquer: Generals* (2003) and its expansion *Zero Hour* (2003). Original game. Original names, art, and audio. Not affiliated with Electronic Arts.

This repo is **design and research only**. There is no game client yet. Stack: Godot **4.8 .NET** ([docs/CONSTRAINTS.md](docs/CONSTRAINTS.md); 4.7.2 until 4.8 is stable). AI orchestrator: [AGENTS.md](AGENTS.md), then [docs/catalog/SLICE.md](docs/catalog/SLICE.md).

## What it is

Three asymmetric factions. You pick a commander. You build a base anywhere on the map, fight over finite supply, spend combat experience on battlefield powers, and try to wipe the other army off the field.

**Goal: multiplayer** (peer-to-peer / LAN, no backend, up to 8). **First step: 1 vs computer.** Same match. No netcode until that is fun.

## Working names

| Role | Name | Analog (source research only) |
| --- | --- | --- |
| High-tech / air / quality | **Aegis** | USA |
| Mass / armor / industry | **Forge** | China |
| Guerrilla / stealth / salvage | **Veil** | GLA |

These names are placeholders. They will not appear in shipping product copy until they survive a later naming pass.

## Docs

| File | What it is |
| --- | --- |
| [docs/VISION.md](docs/VISION.md) | Goal, non-goals, first slice |
| [docs/CONSTRAINTS.md](docs/CONSTRAINTS.md) | No backend, original IP, Godot 4.8 .NET |
| [docs/ROADMAP.md](docs/ROADMAP.md) | What comes after this folder |
| [AGENTS.md](AGENTS.md) | How to read this repo |
| [docs/catalog/README.md](docs/catalog/README.md) | Catalog index |
| [docs/catalog/SCHEMA.md](docs/catalog/SCHEMA.md) | Id prefixes and joins |
| [docs/catalog/INVARIANTS.md](docs/catalog/INVARIANTS.md) | Locked rules |
| [docs/catalog/FACTIONS.md](docs/catalog/FACTIONS.md) | Doctrines and loadouts |
| [docs/catalog/SLICE.md](docs/catalog/SLICE.md) | First playable filter |
| [docs/catalog/MECHANICS.md](docs/catalog/MECHANICS.md) | Systems |
| [docs/catalog/ROLES.md](docs/catalog/ROLES.md) | Units, buildings, powers, upgrades, map objects |
| [docs/catalog/ABILITIES.md](docs/catalog/ABILITIES.md) | Specials and add-ons |
| [docs/catalog/STATUSES.md](docs/catalog/STATUSES.md) | Flags, fields, hole, salvage |
| [docs/catalog/TECH_TREE.md](docs/catalog/TECH_TREE.md) | Prerequisites |
| [docs/catalog/DETECTION.md](docs/catalog/DETECTION.md) | Stealth vs detectors |
| [docs/catalog/SIGHT.md](docs/catalog/SIGHT.md) | Fog, shroud, radar |
| [docs/catalog/DAMAGE.md](docs/catalog/DAMAGE.md) | Damage vs armor |
| [docs/catalog/DELIVERY.md](docs/catalog/DELIVERY.md) | How shots travel |
| [docs/catalog/TERRAIN.md](docs/catalog/TERRAIN.md) | Path tags |
| [docs/catalog/TEAMS.md](docs/catalog/TEAMS.md) | Allies and friendly fire |
| [docs/catalog/HUD.md](docs/catalog/HUD.md) | Screens and HUD |
| [docs/catalog/MATCH_AI.md](docs/catalog/MATCH_AI.md) | Settings and AI |
| [docs/catalog/VOICE.md](docs/catalog/VOICE.md) | Announcer events |
| [docs/catalog/MAP_GRAMMAR.md](docs/catalog/MAP_GRAMMAR.md) | Map layouts, 2–8 players |
| [docs/catalog/MAPS.md](docs/catalog/MAPS.md) | Skirmish map roster |
| [docs/research/SOURCE_GAMES.md](docs/research/SOURCE_GAMES.md) | Generals and Zero Hour as they shipped |
| [docs/research/MECHANICS.md](docs/research/MECHANICS.md) | Source-game systems (narrative) |
| [docs/research/FACTIONS.md](docs/research/FACTIONS.md) | Three doctrines + Zero Hour commanders |
| [docs/research/SOURCES.md](docs/research/SOURCES.md) | Citations |

No numbers. No frozen product names. No app.

## Status

- Game name: **Iron Doctrine**
- App: not started
- Stack: Godot 4.8 .NET. Tick sim, glTF view, lockstep LAN/P2P later. No backend.
- Backend: none, by design
