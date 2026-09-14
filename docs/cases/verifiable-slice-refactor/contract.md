# Frozen seam — verifiable-slice-refactor

Frozen at `e3a01b030a7fb8722f6c83e247e04b6bf1b9aea9` (C). I lanes implement against this; they do not change these rules without an amend.

## Terrain

Catalog `TERRAIN.md`. Helpers on `MapConfig`: `InMap`, `GroundWalkable`, `AirWalkable`, `CanBuildOn`, `TerrainAt`.

- `ter.unbuildable`: ground yes, air yes, build no.
- OOB: `InMap` false; `TerrainAt` returns `ter.block` (not a `ter.unbuildable` walk-block).
- I-sim must path with `GroundWalkable` / `CanBuildOn`. Do not treat unbuildable as a wall.
- Enclosed “no remote action” tests must not use unbuildable as a fence; use buildings or map edge.

## Combat traces

`CombatTrace` / `CombatTracePhase` on `MatchSnapshot.CombatTraces`. Not catalog rows. Instant fire shares `nextShotId` with missiles.

Fog-filter: same as projectiles (comment on the record). Player-visible traces must not leak hidden shots.

Instant: Launch+Impact same tick, same Id, no projectile. Missile: Launch with projectile; Impact or Cancel once; no duplicate Impact.

## Diagnostics

`MovementClassification` {Wait, Stuck, Oscillate} and `MovementDiagnostic` are not snapshot data. Full match must not fail on raw pre-calibration counts.

## Ghost occupancy

Provisional: `CanPlace` blocks visible units. Hidden occupancy must not leak via reason codes.

## Scenario / replay

`tests/ScenarioHarness.cs` ids and `ReplayBundle` field names. GUI proof viewport 1440×900. `Viewport.PushInput` is the only legal GUI driver. ProofPilot is not client-input evidence.

## Assets

No armatures / skeletal walk cycles. Current GLBs with zero glTF animations remain valid. Runtime root bob + chinook `rotor_*` spin is the locomotion policy.

## Red until I-sim

```
dotnet run --project tests/SimProof.csproj -- acceptance
```

Fails (lead-confirmed) for:

1. `path.ground_traverses_unbuildable`
2. empty `CombatTraces` after a legal `del.instant` engagement

Default `bash tools/proof.sh` stays green until I-sim changes pathing (then Movement/RemoteInteractions must be rewritten to the catalog).
