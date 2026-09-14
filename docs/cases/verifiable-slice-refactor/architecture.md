# Architecture note (advisory, restricted `plan` harness)

Architect: `01a0a1e1-f760-7f40-b683-bcbe13a56abd` · grok-4.6 · xhigh · no write tools.
Lead adopted the import-graph facts and the CombatTrace / replay / PushInput recommendations. Lane table follows S (`spec.md`): **one client SCC**, not a concurrent MatchClient/Battlefield split.

## Preserve

Plain C# `IMatch` authority. Godot view. glTF pipeline. SimProof console exe (`dotnet run`, not `dotnet test`). 8-slot model. Placeholder JSON. `--proof-play` is not GUI proof.

## Justified changes

- `ter.unbuildable` walk yes / build no (catalog). Pathing today treats it as a wall (`Pathing.TerrainFits`, `GameConfig.cs:138`).
- Combat traces on snapshot (instant + missile), not Activity+cooldown tracers.
- Sealed order-log replay (additive to twin live FullMatchProof).
- Real `_Input` via `Viewport.PushInput`.
- Runtime locomotion: root bob + chinook rotors; **no** glTF skeletal clips required.
- One `tools/verify.sh`; generalize `review_case.py`.

## Refuse

Second sim/app/renderer. ECS/DI frameworks. Scene-tree authority. Skeletal walk art. New catalog prefixes. Splitting `Match` into multiple public types. Using closed-case `contract.md` as this case’s spec.

## Lane cuts (adopted)

C sequential, then wave 1: I-verify | I-sim | I-client (≤3). Wave 2: I-assets. Hubs: AGENTS.md, csproj, project.godot, placeholders, Bootstrap, case folder.

Architect suggested splitting client-input vs presentation across waves. Lead **did not**: `src/Client/**` is one SCC (`MatchClient`↔`Battlefield`↔`FieldHud`). One I-client lane.

## C freeze checklist

1. `CombatTrace` on `MatchSnapshot`; `nextShotId` used for instant hits too.
2. Terrain walk vs build; OOB is not a catalog `ter.unbuildable` walk-block. Prefer `GroundWalkable` / `CanBuild` helpers; OOB → `ter.block` or `InBounds == false`.
3. Scenario JSON + replay bundle fields + verify group names (stubs).
4. GUI: 1440×900; `PushInput` is the only legal GUI driver (documented in case contract).
5. Asset policy: no armatures; runtime bob + rotors allowed; current GLBs still pass.
6. IMatch-only acceptance tests that are **red for the right reason**.
7. Client must not import `Sim.BehaviorLog` (document; I-client fixes).
8. Review script parameterization belongs to I-verify after types exist.

## Pathing oracles

Do not call production `PlanPath` as the oracle. Independent facts: cell flags, swept footprint vs `TerrainAt` + buildings, per-tick speed cap, air ignores ground clutter, arrival or defined blocked, wait vs stuck criteria frozen by C.

## Replay bundle

revision, patch_hash, config_hash, manifest_hash, toolchain, seed, MatchSetup, inputs.jsonl `{tick,seq,order|pause|input_event}`, checkpoints.jsonl `{tick,state_hash}`, outcome.json with first divergent tick **and field**.
