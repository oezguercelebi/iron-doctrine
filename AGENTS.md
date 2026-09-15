# Agent notes

This repository is a **Godot 4.7.2 .NET** RTS (4.8 .NET when stable). There is already a slice-1 client. Do not scaffold a second app. Catalog is the spec.

## Source of truth (in order)

1. [`docs/catalog/`](docs/catalog/README.md) — what the sim is.
2. [`docs/VISION.md`](docs/VISION.md) and [`docs/CONSTRAINTS.md`](docs/CONSTRAINTS.md) — product bounds.
3. [`docs/research/`](docs/research/SOURCES.md) — analog only. Never ship those names, maps, or copy.

If catalog and research disagree, **catalog wins**.

## Read order

1. [`docs/catalog/SLICE.md`](docs/catalog/SLICE.md) — first match, as a filter, not a second game.
2. [`docs/catalog/SCHEMA.md`](docs/catalog/SCHEMA.md) — id prefixes and how sheets join.
3. [`docs/catalog/INVARIANTS.md`](docs/catalog/INVARIANTS.md) — never invent these.
4. Then the sheet for the system you are touching.

Closed cases record what was built and proved: [`docs/cases/slice-1-playable/`](docs/cases/slice-1-playable/status.md), [`docs/cases/verifiable-slice-refactor/`](docs/cases/verifiable-slice-refactor/status.md). Do not reopen them for a nit. New product work needs a new case folder under `docs/cases/`.

## Architecture

- **Match authority:** plain C# tick simulation (`src/Sim`, `src/Contracts`). Slots, teams, catalog ids, orders, one clock. Not a `Node`.
- **View:** Godot presents a detached snapshot (`src/Client`). Instant tracers come from `CombatTraces`. Chinook rotors and unit bob are presentation; they do not move the sim.
- **Data:** placeholder numbers only in [`data/slice1.placeholders.json`](data/slice1.placeholders.json). Do not put HP/cost/range/XP in catalog sheets.
- **Assets:** original glTF from Blender. No armatures. Runtime may spin listed assemblies.

Do not move authority into the scene tree or Godot MultiplayerAPI. Do not add PlayFab / UGS / Photon / EOS. Do not delete slots, teams, or loadout ids to make a slice compile. Do not implement the future catalog (Forge/Veil expansion, LAN, lobby, backend, commander product names).

## Catalog rules

- Use catalog **ids** in data. Do not invent prefixes or rows.
- Working faction names (Aegis, Forge, Veil) are placeholders. Commander product names are not chosen. Ids (`aegis.air`, …) are stable.
- The sim is a **match** even when only 1 vs computer is built.
- Stack is locked in [`docs/CONSTRAINTS.md`](docs/CONSTRAINTS.md).

## Verification

Run from the repo root. Passing one layer does not establish another. Missing, skipped, timed out, or unavailable required checks are pending or failed, never passed. Do not invent a command or claim a missing gate ran. Do not substitute `dotnet test` for the custom executables.

| Command | Coverage |
| --- | --- |
| `bash tools/verify.sh` | Named scenarios; structured JSON; fail-closed. Audit is report-only. Render is pending without a display. |
| `bash tools/proof.sh` | Simulation, atomics, sealed replay. Console exe, not `dotnet test`. |
| `bash src/Client/Proof/run.sh` | Command-intent helpers. Not Godot mouse/HUD/render. |
| `bash src/Client/Proof/verify-godot.sh` | Real `InputEvent`s through production `_Input`. Headless is dispatch only. |
| `python3 tools/art/verify_assets.py` | GLB structure. Runtime bob/rotors are client checks. |
| `bash tools/audit.sh` | Diagnostics. Stuck/oscillate alerts are not a full-match fail gate. |
| `bash tools/run.sh` | Version, build, import, launch. Launch is not gameplay proof. |

Behavior map: [`docs/cases/verifiable-slice-refactor/acceptance-matrix.md`](docs/cases/verifiable-slice-refactor/acceptance-matrix.md). How to add a contract and replay a failure: [`docs/cases/verifiable-slice-refactor/workflow.md`](docs/cases/verifiable-slice-refactor/workflow.md).

`python3 tools/review_case.py --case <id>` is a restricted reviewer helper. Process exit 0 is not an APPROVE verdict.

## Behavior contracts

For a changed gameplay or presentation behavior, record before building:

- **Rule:** owning catalog row. Scenario names are not new gameplay ids.
- **Scenario:** initial state, catalog ids, seed if any, ordinary input or order.
- **Assertions:** observable and forbidden outcomes, deadline, justified tolerance.
- **Proof:** command, layers, artifacts, explicitly unverified claims.

If code, tests, and catalog disagree, fix the implementation or amend the catalog row. Do not bless a bug by copying it into expected results. After measurement starts, do not teleport, apply direct damage, or mutate internals as a stand-in for the action under test.

## Never

See [`docs/catalog/INVARIANTS.md`](docs/catalog/INVARIANTS.md). Short list: no Veil power or airfield, no navy, no construction yard, no backend, no EA marks, no 1v1-only engine.
