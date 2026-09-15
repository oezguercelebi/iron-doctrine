# Iron Doctrine

A modern-warfare real-time strategy game. You plant a base on the map, fight over finite supply, and win by destroying every enemy building. Losing an army is recoverable.

Original game, models, map geometry, and audio. Research notes in [`docs/research/`](docs/research/SOURCES.md) explain the 2003 analogs. They are not the spec and must not appear as shipping names, maps, or art.

**Goal:** multiplayer on LAN / peer-to-peer, no backend, up to eight slots. **Now:** one human vs one computer. Same match rules. No netcode until that match is fun.

## Play

Requires **Godot 4.7.2 .NET** (4.8 .NET when stable) and **.NET SDK 8.0.425** (or a compatible .NET 8 feature band). A Godot build without .NET will not run this project.

```sh
./tools/run.sh
```

The launcher uses ignored toolchains in `.tools/` when present. Otherwise put `dotnet` on `PATH` and set `IRON_GODOT` to the Godot .NET executable. It checks the engine version, builds C#, imports meshes, and starts the match.

You get Command and Dozer against a medium Aegis AI on the two-flat map. Select the Dozer, build Fusion, then a Drop-off by a supply dock, then Barracks and Factory. Destroy every enemy building. **H** is the in-game guide. **Escape** pauses. Rematch is on the result screen. [Controls](src/Client/README.md).

Balance numbers are placeholders in [`data/slice1.placeholders.json`](data/slice1.placeholders.json). Faction names Aegis / Forge / Veil are working labels, not final product copy.

## How it is built

The match is a plain C# tick simulation: slots, teams, catalog ids, orders, one clock. Godot presents a detached snapshot. The scene tree is not the authority. Later multiplayer is meant to be lockstep over ENet, not scene replication. [Constraints](docs/CONSTRAINTS.md).

Slice-1 implements the first local match (closed case [`slice-1-playable`](docs/cases/slice-1-playable/status.md)). Verification contracts, sealed replay, real client input, combat traces, and runtime locomotion are in closed case [`verifiable-slice-refactor`](docs/cases/verifiable-slice-refactor/status.md).

## Verify

From the repository root. Do not substitute `dotnet test` for these runners.

```sh
bash tools/verify.sh          # list/select scenarios; fail-closed local gate
bash tools/proof.sh           # simulation + sealed replay
bash src/Client/Proof/run.sh  # command-intent helpers (not Godot mouse)
bash src/Client/Proof/verify-godot.sh  # real InputEvents through the client
python3 tools/art/verify_assets.py
bash tools/audit.sh           # diagnostic report; not a stuck/oscillate fail gate
```

Scenario map: [`docs/cases/verifiable-slice-refactor/acceptance-matrix.md`](docs/cases/verifiable-slice-refactor/acceptance-matrix.md). Agent/contributor rules: [`AGENTS.md`](AGENTS.md).

## Docs

| File | What |
| --- | --- |
| [`docs/VISION.md`](docs/VISION.md) | Goal and first slice |
| [`docs/CONSTRAINTS.md`](docs/CONSTRAINTS.md) | No backend, original IP, stack |
| [`docs/ROADMAP.md`](docs/ROADMAP.md) | After this folder |
| [`docs/catalog/`](docs/catalog/README.md) | Spec. Catalog wins over code and research |
| [`docs/cases/`](docs/cases/) | Closed engineering cases and evidence |
| [`docs/research/`](docs/research/SOURCES.md) | Analog only |

Catalog sheets have no HP, cost, range, or XP numbers. Temporary tuning lives in the placeholder JSON file.

## License

[MIT](LICENSE). Original assets under the same license. Do not import Command & Conquer / Generals / Zero Hour names, maps, or art.
