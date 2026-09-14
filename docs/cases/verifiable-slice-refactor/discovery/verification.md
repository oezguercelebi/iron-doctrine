# Discovery: verification / tooling / workflow

Explore `01a0a1dc-dff1-70a2-8500-6123df9cb983` · grok-4.5 · effort high · read-only.

## Commands (real scope)

| Command | Pass means | Not covered |
|---|---|---|
| `tools/proof.sh` | SimProof exe, all groups, paired StateHash | Godot, GUI, render, anim, stuck as gate |
| `src/Client/Proof/run.sh` | CommandIntent/SelectionIntel helpers | Real mouse/HUD/render |
| `python3 tools/art/verify_assets.py` | GLB structure; **no animations** asserted | Runtime anim, Godot import unless Blender |
| `tools/audit.sh` | Writes report; **exit 0 even with issues** | Not an acceptance gate |
| `tools/run.sh` | Version, build, import, launch | Launch ≠ gameplay proof |

`dotnet test` is not used and must not be silently substituted.

## `tools/review_case.py`

Hardcodes `docs/cases/slice-1-playable`. `--proof` is unvalidated `read_text`. Exit code is Codex process code, **not** parsed APPROVE/BLOCK. Write tools stripped for Codex only. No working-tree/config/asset hashes. Generalization is in scope.

## Missing

Replay packages, atomic scenario selector, structured outcomes, negative-control worktrees, render gates, GUI gates, hosted CI (none; not the acceptance path), locomotion animation proof, stuck as calibrated gate.

## Toolchain

Godot 4.7.2 .NET at `.tools/godot`. SDK 8.0.425. Blender at `.tools/blender-mount/Blender.app/Contents/MacOS/Blender`. Renderer `gl_compatibility`. No `.github`.

## Foundation lane files (suggested)

`tools/verify.sh` (or extend `proof.sh`), `tests/Program.cs` selector, scenario format, structured JSON writer, replay schema, `tools/review_case.py` parameterization. Hub: AGENTS.md command table, case folder, lockfiles.
