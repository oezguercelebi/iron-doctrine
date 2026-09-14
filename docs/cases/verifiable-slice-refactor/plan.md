# Plan — verifiable-slice-refactor

Max three I lanes concurrent. Shared file or import edge without freeze ⇒ one row. Hubs = lead.

## Contract freeze (first)

- [x] `c-freeze` done · lane `src/Contracts/MatchContract.cs` `src/Contracts/GameConfig.cs` `tests/SeamConformance.cs` `tests/AcceptanceContracts.cs` `tests/ScenarioHarness.cs` `tests/Program.cs` `tools/art/verify_assets.py` · after — · Merged `e3a01b0`. Acceptance red for unbuildable traverse + empty CombatTraces. proof.sh green.

## Implementation (after freeze)

Recommended wave 1 (≤3): `i-verify`, `i-sim`, `i-client`. Wave 2: `i-assets`. Then lead integrate.

- [ ] `i-verify` open · lane `tools/verify.sh` `tools/review_case.py` `tests/Program.cs` `tests/ReplayPackage.cs` `tests/VerifyOutcomes.cs` · after c-freeze · Add fail-closed `tools/verify.sh` (list/select scenarios, structured outcomes). Wire Program selector to frozen ScenarioHarness API. Sealed replay package read/write. Generalize `review_case.py` (parameterized case id, proof identity, parse APPROVE/BLOCK, missing proof ≠ pass). Keep `dotnet run --project tests/SimProof.csproj` as the exe; do not switch to `dotnet test`.

- [ ] `i-sim` open · lane `src/Sim/**` `tests/MechanicsProof.cs` `tests/AtomicScenarios.cs` `tests/BehaviorAudit.cs` `tests/FullMatchProof.cs` · after c-freeze · Repair `TerrainFits`/`CanPlace` for catalog `ter.unbuildable`. Emit shot causal fields. Calibrate stuck/oscillate vs legitimate wait; audit reports without failing full match on raw heuristic counts. Author atomic sim scenarios from `contracts-outline.md` / matrix. Keep FullMatchProof as public-order match; **add** sealed-input replay twin. Preserve slot/team/loadout model.

- [ ] `i-client` open · lane `src/Client/**` · after c-freeze · Single client SCC lane. Production `_Input` injection path for named client scenarios; development control surface (load scenario, step ticks, inspect, frame capture) inside existing client — not a second app. HUD/world fog leak tests. Instant tracers only when correlated to a resolved shot; missiles keep projectile ids. Root motion + facing for infantry/vehicles; spin existing `rotor_*` on chinook; structures static. Extend Proof helpers only inside this glob; Godot path is required for matrix client rows.

- [ ] `i-assets` open · lane `assets/models/**` `assets/sources/**` `tools/art/build_assets.py` `tools/art/verify_assets.py` · after c-freeze · Implement frozen animation policy: no armatures / no skeletal walk cycles; optional allowlisted root/rotor clips only per `locomotion-requirements.md`. Preserve current visual direction. Rebuild GLBs; verify_assets green under new policy.

## Lead / integrate

- [ ] `lead-hubs` open · lane — · after c-freeze · Point `AGENTS.md` verification table at `tools/verify.sh` and this case’s evidence rules. No product logic.

- [ ] `integrate-proof` open · lane — · after i-verify, i-sim, i-client, i-assets, lead-hubs · On main: `bash tools/verify.sh`, `bash tools/proof.sh`, `bash src/Client/Proof/run.sh`, `python3 tools/art/verify_assets.py`, `bash tools/audit.sh` (calibrated reporting). Record revision, hashes, artifacts. Negative-control worktrees for critical new gates. Assemble user visual/play package. Stop at ready-to-close after case R; user closes. No push.
