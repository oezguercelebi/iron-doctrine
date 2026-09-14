# Baseline command runs (lead, before structural edits)

Tree: `c5aefdd7ac1430b29361f259cd135f3a833d80d7` on Darwin arm64.
Toolchain: Godot `4.7.2.stable.mono.official.ed1daf0bf` at `.tools/godot/Godot_mono.app`; dotnet `8.0.425` at `.tools/dotnet`. No CI. No `.github`. Blender binary present at `.tools/blender-mount/Blender.app/Contents/MacOS/Blender` (not on PATH as `blender`).

## `bash tools/proof.sh`

Exit 0. Custom `dotnet run --project tests/SimProof.csproj`, not `dotnet test`.

Observed groups (all PASS):
- seam, submission timing
- mechanics: construction, production, dock/loading, air targeting/missiles, crush/force-attack, garrison, scout fire-out/transport, capture, repair, veterancy/self-heal, fog, terrain pathing, eight-slot/unfinished/elimination, AI hidden-info
- review regressions (builder footprint, blocked remote actions, occupied garrison, powered defenses, cargo, unload reach, ghost pad, scaffold leave, rally facing)
- order boundary
- full match: human-v-AI, 17 public player orders, AI jobs, defeat at tick 2033, deterministic replay hash `AFF97B33D0FECB7208E09F6869EDA724E97384863B4137450AD977D751317842`, elapsed 5.6s

Does not cover: real Godot mouse/HUD/rendering, animation, stuck/oscillation as a failing gate.

## `bash src/Client/Proof/run.sh`

Exit 0. Temporary net8 console project compiling Contracts + Sim + CommandIntent + SelectionIntel + ClientInputProof. Defines `CLIENT_INPUT_PROOF`. Ends `CLIENT_R2_TARGETED_PROOF_OK`.

Does not execute Godot, mouse, HUD regions, or rendering.

## `python3 tools/art/verify_assets.py`

Exit 0. `PASS 15 original assets; 100785 triangles; file validation (run in Blender for import proof)`.
`verify_assets.py:34` asserts GLBs have **no animations** (`assert not doc.get("animations")`). `tools/art/build_assets.py` exports with `export_animations=False`. This check is structure-only in CPython.

## `bash tools/audit.sh`

Exit 0. Writes `artifacts/behavior-audit.txt`.
`BEHAVIOR_AUDIT tick=2033 phase=Finished winners=1`
`BEHAVIOR_REPORT stuck=12 oscillate=42 fallback_ticks=693 box=0`
`BEHAVIOR_ISSUES stuck=12 oscillate=42`
Historical lead confirmed on this tree: a finished deterministic full match coexists with stuck/oscillation diagnostics. Audit does **not** fail the process on those alerts.

## `bash tools/run.sh`

Not executed at baseline (launches the visible client). Script checks Godot 4.7.2/4.8 .NET, `dotnet build IronDoctrine.csproj`, headless import, then `exec` Godot. Launch alone is not gameplay evidence.

## Still missing at this tree

Atomic scenario selector, structured per-scenario JSON, replay bundles, GUI/render gates, negative-control worktree evidence, generalized `tools/review_case.py` (still hardcodes `slice-1-playable`).
