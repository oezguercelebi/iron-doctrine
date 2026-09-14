## 2026-09-14T21:38Z user
Authorized comprehensive evidence-driven refactor of the existing Godot slice. New case `verifiable-slice-refactor`. Do not reopen `slice-1-playable`. Accept of outcome, limits, and defaults in the prompt. No push. User closes. Visual/play acceptance remains user.

## 2026-09-14T21:38Z lead
Opened `verifiable-slice-refactor`. `slice-1-playable` remains closed.

Starting tree:
- HEAD `c5aefdd7ac1430b29361f259cd135f3a833d80d7` (`Update AGENTS.md`)
- `main` ahead of `origin/main` by that one commit
- working tree clean
- Conversation-start snapshot listed `M AGENTS.md`; that update is already committed as HEAD and is the shared base. Nothing else uncommitted. No unrelated dirty files to preserve beyond this commit.
- Crew lock empty. No other open case.

Model/effort mapping (Grok harness; spawn tool has `model` but no `effort` field — effort is written into each child prompt):
- Explore: `explore` · `grok-4.5` · effort `high`
- Spec (S): `general-purpose` · `grok-4.5` · effort `high` · writes `docs/cases/verifiable-slice-refactor/` only
- Architect: `plan` · `grok-4.6` · effort `xhigh` · no write tools
- Implement/Debug: `general-purpose` · `grok-4.6` · effort `xhigh` · one lane, isolation worktree on disk
- Contract (C): `general-purpose` · `grok-4.6` · effort `high` · named seam + acceptance tests
- Review (R): `pr-review-toolkit:code-reviewer` plus generalized `tools/review_case.py` · `grok-4.6` · effort `xhigh` · no write tools
- Codex-class seats, if used: `gpt-6-astra` with the same effort column. `tools/review_case.py` currently hardcodes `slice-1-playable` and `gpt-6-astra`; generalizing it is in scope.
- No Haiku-class / cheapest model. No silent downgrade.

Budget (authorized):
```
patch-rounds: 2
deviations: in-lane
unanswered: default
push: never
close: user
```

Spawned three independent discovery explores in one message (sim/contracts/determinism; client/input/presentation/assets; verification/tooling/workflow). Also started existing proof commands for a real baseline. Spec/plan not written until discovery returns.

## 2026-09-14T21:45Z lead
Baseline commands on `c5aefdd` (see `proof/baseline-commands.md`):
- `bash tools/proof.sh` exit 0; full match defeat tick 2033; replay hash `AFF97B33D0FECB7208E09F6869EDA724E97384863B4137450AD977D751317842`
- `bash src/Client/Proof/run.sh` exit 0 `CLIENT_R2_TARGETED_PROOF_OK` (plain C#, no Godot)
- `python3 tools/art/verify_assets.py` exit 0; 15 assets; GLBs asserted to have no animations (`verify_assets.py:34`)
- `bash tools/audit.sh` exit 0; `stuck=12 oscillate=42` on the same finished match; alerts are not a gate
- `tools/run.sh` not launched (visible client). Godot 4.7.2 .NET present. Blender present under `.tools/blender-mount/`
- Historical lead still true: passing full match coexists with stuck/oscillation diagnostics. Animation exclusion still true in asset contract.

## 2026-09-14T21:50Z lead
Discovery returned (three explores, one message). Reports in `discovery/`. Confirmed: `ter.unbuildable` catalog drift; ProofPilot bypasses GUI; instant tracers not shot-correlated; review_case.py hardcodes slice-1-playable and treats process exit as success. Spawned S (`general-purpose` grok-4.5 high, case folder only) and architect (`plan` grok-4.6 xhigh, no write) together.

## 2026-09-14T22:00Z lead
S wrote spec/plan/matrix/contracts-outline/locomotion-requirements. Architect note saved as `architecture.md`.

Lane cut adopted from S: wave1 I-verify | I-sim | I-client (whole `src/Client/**` SCC). Architect’s sequential presentation split not used.

Provisional defaults Q1–Q10 taken as spec. Additional: `TerrainAt` OOB should not be used as a walk-block synonym for `ter.unbuildable`; C adds walk/build helpers.

C is next. Shared base will be a local commit of this case folder, then a real worktree `.worktrees/c-freeze`.

## 2026-09-14T22:20Z s
Spec accepted-write: `spec.md`, `plan.md`, `acceptance-matrix.md`, `contracts-outline.md`, `locomotion-requirements.md`; status amended. Lanes: c-freeze → i-verify / i-sim / i-client (wave1) → i-assets → lead-hubs / integrate-proof. C seams: terrain unbuildable, shot causal fields, replay/scenario API, wait-vs-stuck, provisional ghost, animation allowlist. Proof: keep proof.sh / Client Proof / verify_assets / audit; add `bash tools/verify.sh` fail-closed. Q1–Q10 defaults taken (authorized). Budget unchanged. Row count 7. Ready for C freeze.
