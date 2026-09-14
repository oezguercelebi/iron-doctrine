# Workflow for the next agent

Case: `verifiable-slice-refactor`. Catalog wins. Do not reopen `slice-1-playable`.

## Commands

From repo root, pinned toolchain in `.tools/` (dotnet 8.0.425, Godot 4.7.2 .NET, Blender under `.tools/blender-mount/`).

| Want | Command |
| --- | --- |
| Full local gate | `bash tools/verify.sh` |
| List scenarios | `bash tools/verify.sh --list` |
| One scenario | `bash tools/verify.sh --scenario <id>` |
| Sim only | `bash tools/proof.sh` |
| Frozen C contracts | `dotnet run --project tests/SimProof.csproj -- acceptance` |
| Helper input (not GUI) | `bash src/Client/Proof/run.sh` |
| Real Godot dispatch | `bash src/Client/Proof/verify-godot.sh` |
| Assets | `python3 tools/art/verify_assets.py` |
| Audit report | `bash tools/audit.sh` |
| Play | `bash tools/run.sh` |
| Restricted R | `python3 tools/review_case.py --case verifiable-slice-refactor <name> <base> <head> --lane <glob> --proof <file>` |

`dotnet test` is not the harness. Process exit 0 from a reviewer is not APPROVE.

## Add a behavior contract

1. Catalog sheet + id (no new gameplay prefixes).
2. Row in `acceptance-matrix.md` and block in `contracts-outline.md`.
3. Executable IMatch (or PushInput) check. After measurement: no teleport/Hit-as-action.
4. Wire `tools/verify.sh --scenario`.
5. Independent R on the revision that added it.

## Replay a failure

Sealed bundles use `ReplayBundle` field names (`tests/ReplayPackage.cs`). Replay Submit/SetPaused/Step only. First divergent tick (and field when hashed).

## Visual evidence

Headless Godot is dispatch only. Frames need a display and `visual-play.md`. Do not auto-approve golden images. User accepts feel at close.

## Which gates a change affects

See `acceptance-matrix.md`. Pathing → acceptance + proof.sh + maybe GUI move. Combat traces → acceptance + client tracers. HUD → verify-godot.sh. Assets → verify_assets.py. Review tooling → `review_case.py --self-check`.
