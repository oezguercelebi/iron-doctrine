# Iron Doctrine

A real-time strategy game of **named commanders and clashing doctrines**.

Original game, models, map geometry, and audio. Research references live in docs/research and do not define the playable content.

The accepted [slice-1-playable case](docs/cases/slice-1-playable/status.md) implements the first local match. Engine baseline: **Godot 4.7.2 .NET**, moving to 4.8 when stable. The simulation is plain C#, driven by one clock and submitted orders; Godot renders the result. [Constraints](docs/CONSTRAINTS.md).

## What it is

Three asymmetric factions. You pick a commander. You build a base anywhere on the map, fight over finite supply, spend combat experience on battlefield powers, and win by destroying every enemy building. Losing an army is recoverable.

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

Catalog sheets contain no balance numbers or frozen product names. All temporary gameplay tuning lives in [data/slice1.placeholders.json](data/slice1.placeholders.json).

## Status

- Game name: **Iron Doctrine**
- App: playable slice-1 (1vAI). Case [slice-1-playable](docs/cases/slice-1-playable/status.md) closed.
- Stack: Godot 4.8 .NET. Tick sim, glTF view, lockstep LAN/P2P later. No backend.
- Backend: none, by design

## Run locally

Diagnostics: in-game **F3** overlay, or `bash tools/run.sh -- --diag`, or headless `bash tools/audit.sh` (writes `artifacts/behavior-audit.txt`).

Requires Godot **4.7.2 .NET** and .NET SDK **8.0.425** (or a compatible .NET 8 feature band). A standard Godot build without .NET support will not work. No network service is required by the match.

```sh
./tools/run.sh
```

The launcher finds the local ignored toolchains in `.tools/`. With your own installations, put `dotnet` on PATH and set `IRON_GODOT` to the Godot .NET executable. It checks the engine version, builds C#, imports meshes, and starts the match.

```sh
./tools/proof.sh
```

The match boots directly into the two-slot skirmish: your Command and Dozer against the medium AI. Select the Dozer, build Fusion, then a Drop-off by a supply dock, Barracks and Factory. Destroy every enemy building to win. Press **H** for the in-game controls guide, **Escape** to pause/resign, and use **Rematch** on the result screen. [Full controls](src/Client/README.md).

All balance values are placeholders. Optional AA linking is deferred. The final integrated visible proof reached victory at02:15; manual checks cover construction, production/refunds, selection, pause, resign and rematch. [Acceptance evidence and two open review findings](docs/cases/slice-1-playable/acceptance.md).

The proof executable runs the plain C# simulation independently of Godot. Proof records and independent review verdicts live in [the case folder](docs/cases/slice-1-playable/).
