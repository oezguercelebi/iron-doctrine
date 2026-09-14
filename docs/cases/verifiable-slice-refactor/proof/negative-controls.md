# Negative controls (lead, isolated worktrees, not merged)

Base: `3c437eb` (i-sim merged). Fault trees discarded.

## Unbuildable treated as a wall

Worktree `.worktrees/neg-unbuildable`. Patch: `TerrainFits` requires `id != "ter.unbuildable"`.

`dotnet run --project tests/SimProof.csproj -- acceptance` exit 134:

```
FAIL acceptance: Acceptance path.ground_traverses_unbuildable: TERRAIN.md ter.unbuildable is ground-walkable; unit never occupied or traversed that cell
PASS combat.instant_tracer_tied_to_shot
```

Compile succeeded; the failure is the catalog claim, not a build error.

## CombatTraces omitted from snapshot

Worktree `.worktrees/neg-traces`. Patch: `Intel.Snapshot` sets `CombatTraces = Array.Empty<CombatTrace>()`.

Exit 134:

```
PASS path.ground_traverses_unbuildable
FAIL acceptance: MatchSnapshot.CombatTraces empty after del.instant engagement
```

## Not run here

- Duplicate missile impact fault (atomic exists on the good tree).
- `armor.basic` damaging chinook (atomic `combat.tank_cannot_hit_chinook` on the good tree).
- HUD chrome leaking Move (Godot `client.no_fog_world_leak` on i-client).
- Disconnected locomotion (no frame gate yet; user visual accept).
