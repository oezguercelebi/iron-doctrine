# Spec — verifiable-slice-refactor

Accepted 2026-09-14. Catalog wins over tests and closed-case notes.

## Outcome

Make the existing Godot slice-1 client evolvable with bounded, reproducible evidence: catalog-backed atomic contracts, one fail-closed verify entry point, sealed replay + selectable scenarios, real client input path, calibrated stuck/wait diagnostics, instant-shot presentation correlation, and minimal locomotion presentation (root motion + chinook rotors). Defects against the catalog are fixed. No second app. No universal-correctness claim.

## Non-goals

Future catalog (Forge/Veil, specialists, LAN, lobby, backend, commander naming, final balance). Second application. Engine migration. `dotnet test` as the runner. Hosted CI as acceptance. New dependencies. Push. Deleting slots/teams/loadout ids. Rewriting TERRAIN.md to match the `ter.unbuildable` bug. New skeletal walk-cycle art direction. Cross-GPU pixel identity.

## Lanes (real globs)

| Lane | Glob | Notes |
| --- | --- | --- |
| **C** | `src/Contracts/MatchContract.cs`, `src/Contracts/GameConfig.cs`, `tests/SeamConformance.cs`, `tests/ScenarioHarness.cs` (new stub), `tools/art/verify_assets.py` (animation-policy asserts only) | Freeze only. Failing stubs. No feature fill. |
| **I-verify** | `tools/verify.sh` (new), `tools/review_case.py`, `tests/Program.cs`, `tests/ReplayPackage.cs` (new), `tests/VerifyOutcomes.cs` (new) | Selector, structured JSON, sealed replay I/O, review harness generalization. No `src/Sim/**` internals. |
| **I-sim** | `src/Sim/**`, `tests/MechanicsProof.cs`, `tests/AtomicScenarios.cs` (new), `tests/BehaviorAudit.cs`, `tests/FullMatchProof.cs` (extend only: keep public-order match; add sealed-replay twin) | Pathing/combat/stuck vs frozen contracts + atomics. |
| **I-client** | `src/Client/**` | One lane: MatchClient↔Battlefield↔FieldHud↔audio/selection/pilot are one SCC. Real `_Input` injection, HUD/world leak checks, development control surface, shot-correlated tracers, root motion + rotor spin. |
| **I-assets** | `assets/models/**`, `assets/sources/**`, `tools/art/build_assets.py`, `tools/art/verify_assets.py` (fill after C policy) | Allowlisted locomotion only; no armatures. |
| **Lead** | `docs/cases/verifiable-slice-refactor/**`, `AGENTS.md` (command table pointer), merges, `tools/proof.sh` / `tools/audit.sh` wrappers if needed, negative-control worktrees, visual package | Hubs stay lead-owned. |

Do not give I: `AGENTS.md`, lockfiles, package manifests, `data/slice1.placeholders.json` (unless lead patches a labeled placeholder), catalog sheets, or the case folder.

## Seams C must freeze

1. **Terrain policy** — `ter.unbuildable`: ground-traversable, build-forbidden (`TERRAIN.md`). `GameConfig` comment + `CanPlace`/`TerrainFits` contract; SeamConformance red for current impassable ground bug.
2. **Shot / order causal fields** — diagnostic fields on existing snapshots/events (not new catalog rows): correlate resolved instant hit and missile `ProjectileSnapshot.Id` to presentation.
3. **Replay + scenario API** — sealed input log + selectable named scenarios; companion types/helpers in Contracts + `ScenarioHarness` stubs. FullMatchProof remains public-order; sealed replay is additive.
4. **Wait vs stuck classification** — diagnostic criteria API; audit may report; must not fail full match solely on current heuristic counts until calibrated.
5. **Ghost occupancy (provisional)** — CanPlace blocks visible units (slice-1 provisional); document in seam test as provisional.
6. **Asset animation policy** — verify_assets: forbid skeletal/armature walk cycles; allowlist client-or-glTF root/rotor locomotion per `locomotion-requirements.md`.

## Proof (per row / after verify lane)

Keep: `bash tools/proof.sh` · `bash src/Client/Proof/run.sh` · `python3 tools/art/verify_assets.py` · `bash tools/audit.sh`.

Add: **`bash tools/verify.sh`** — lists/selects named scenarios; structured outcomes; **fails closed** on required missing checks. Does not replace SimProof with `dotnet test`.

Matrix: [`acceptance-matrix.md`](acceptance-matrix.md). Contracts: [`contracts-outline.md`](contracts-outline.md). Locomotion: [`locomotion-requirements.md`](locomotion-requirements.md).

Lane proof one-liners:
- C: `dotnet run --project tests/SimProof.csproj -- seam` (or verify seam group) shows **red** for frozen stubs for the right reason.
- I-verify: `bash tools/verify.sh --list` and `--scenario <id>` fail-closed; `python3 tools/review_case.py --help` accepts parameterized case; missing proof ≠ pass.
- I-sim: `bash tools/verify.sh --group sim` (atomics + mechanics) + existing `bash tools/proof.sh`.
- I-client: Godot-backed input/injection scenarios named in matrix + `bash src/Client/Proof/run.sh` still green for helpers.
- I-assets: `python3 tools/art/verify_assets.py` (+ Blender import when available).
- Integrate (lead): full `bash tools/verify.sh`, `bash tools/proof.sh`, audit (non-gating until calibrated), negative-control worktree notes, user visual package.

## Questions (defaults taken — authorized)

| Id | Question | Options / cost | Default |
| --- | --- | --- | --- |
| Q1 | `ter.unbuildable` | A repair pathing/CanPlace · B rewrite TERRAIN.md | **A** repair to catalog |
| Q2 | Ghost occupancy | A keep visible-unit CanPlace block (provisional) · B full fog-safe occupancy now | **A** provisional |
| Q3 | Locomotion | A root motion + chinook rotors · B new skeletal walk art | **A** |
| Q4 | Shot/order causal ids | A diagnostic fields on snapshots/events · B new catalog rows | **A** |
| Q5 | Stuck/oscillation | A calibrate + distinguish wait before gating · B fail full match on current counts | **A** calibrate first |
| Q6 | FullMatchProof | A keep public-order + add sealed replay/atomics · B replace with `dotnet test` | **A** |
| Q7 | Instant tracers | A must correlate with resolved shot when claiming combat FX · B Activity+cooldown only | **A** correlate |
| Q8 | Slots/teams/loadouts | A preserve ids · B delete unused to simplify | **A** preserve |
| Q9 | Deps / engine / push / CI | A none · B add | **A** none; push never; no hosted CI acceptance |
| Q10 | `review_case.py` | A generalize (case, proof identity, parse APPROVE/BLOCK, missing≠pass) · B leave hardcoded | **A** generalize |

## Budget

```
patch-rounds: 2
deviations: in-lane
unanswered: default
push: never
close: user
```
