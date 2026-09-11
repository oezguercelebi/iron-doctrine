# Log

## 2026-09-11 accept
User accepts this build case and lifts scaffold ban. Starting commit 63a008f on main; clean checkout. Crew read at ~/.claude/skills/crew/SKILL.md. No app or runtime detected initially. Godot official release API confirms 4.7.2-stable; 4.8 is development per official blog. No engine downgrade.

## Lane table before spawn
contract → isolated worktree, proposal only, shared hub adopted by lead; art → assets/** and tools/art/** isolated worktree. Independent wave. sim → src/Sim/**, tests/** and client → src/Client/**, scenes/** after frozen API. Three implementation lanes maximum.
Model mapping: collaboration exposes OpenAI models, not the Claude/Grok aliases in AGENTS; set gpt-6-astra explicitly with high for C and xhigh for I. R will use local Claude CLI, fable/xhigh, with only Read/Glob/Grep tools, enforcing no writes. Lead executes proof and supplies raw artifacts because Bash would grant R write capability.

## 2026-09-12 tooling
Godot executable reports 4.7.2.stable.mono.official.ed1daf0bf; .NET SDK 8.0.425 installed under ignored .tools/. Godot.NET.Sdk/4.7.2 restores successfully. Local launch and proof wrappers scaffolded, no system path edits.
Claude/fable reviewer probe failed: organization disabled Claude subscription access. Alternative verified via fresh Codex CLI with gpt-6-astra/xhigh, read-only sandbox, model catalog apply_patch_tool_type=null, shell/JS/apps/plugins/image generation/multi-agent disabled and model multi_agent_version=v1. Tool probe exposed only functions exec/wait/request_user_input plus read-only view_image. R receives complete numbered source/diff, current case/catalog and raw proof output via stdin; no write or shell tools. This is a harness adaptation, not author self-review.

## 2026-09-12 C freeze and next wave
Adopted C proposal into src/Contracts/**, data/slice1.placeholders.json, frozen tests/SeamConformance.cs, contract.md. Frozen sim factory and client Initialize seam. Shared contract compiles; executable proof initially fails CS0234 (IronDoctrine.Sim absent) as expected, saved proof/contract-red.txt. No feature sim exists yet. Client and sim fan out together from this committed HEAD; art continues as independent third I. tests/SimProof.csproj is a lead-owned manifest; SeamConformance.cs is frozen; sim may add other tests and extend Program.cs. Lanes locked in plan before spawn.

## 2026-09-12 review dispatch
Fresh Crew R contract spawn dispatched immediately after adoption commit 09d071a. Range b60816f..09d071a, raw red proof supplied, no builder narrative. Reviewer output pending in reviews/contract.md. Sim/client spawned together with explicit gpt-6-astra/xhigh in isolated worktrees.

## 2026-09-12 integration preparation
Added C# stack gate mapping in acceptance.md; Crew TypeScript example is inapplicable. Lead composition root wires IMatchFactory and MatchClient only. README now identifies the accepted scaffold and local launch/proof entry points; gameplay claims remain pending until proof. No push.

## 2026-09-12 R contract — block, patch round 1
R returned P2 tools/review_case.py:34: numbered source used working tree while diff used requested commit. Patched harness to read source with git show <head>:<path>. R also found a proof gap: frozen seam lacks pre-Step gameplay assertion. Added a supplemental proof task in the sim lane; frozen SeamConformance.cs remains unchanged and the accepted next-tick contract remains unchanged. No user decision is needed for these reversible in-scope fixes. Fresh R follows the patch. Missing-factory red accepted as appropriate at this phase.

## 2026-09-12 early proof merge
Lane check main...codex/slice1-sim returned only tests/SubmissionTiming.cs. Merged 3fb7f18 while sim implementation remains in its worktree. Supplemental next-tick/input-detachment proof added without modifying frozen test. Fresh R contract-patch spawned on merged range immediately; original missing-factory red remains expected.

## 2026-09-12 art progress
Art reports all 15 GLBs and original Blender sources generated. Source/preview import exclusions being added; correcting rocks floor bound and triangle count proof before return. Lead inspected rendered tank and gatherer previews for integration readiness; independent R still required. Catalog data id check found 15 slice roles, eight slots, and no ids absent from catalog.

## 2026-09-12 R contract-patch — block, patch round 2
R found P1 src/Bootstrap/Main.cs:16 ambiguous FileAccess under implicit System.IO; lead qualifies Godot.FileAccess (fix prepared before R return). P2 tests/Program.cs:2 did not yet invoke supplemental proof; proof gap only tested Move, not Build/debit. Sim child assigned actual invocation and Build timing checks in the supplemental file. Original frozen proof remains unchanged. Both R verdicts preserved. Fresh R waits for this concrete patch commit, no change to accepted outcome/proof semantics.

## 2026-09-12 art merge and R
Art b110c4a passed lane check: 53 paths, assets/** and tools/art/** only. Merged on main; reran full Blender verification on main: 15/15 original GLBs imported, normals/materials/indices/floor/bounds pass, 100785 total triangles. Raw output proof/art-import.txt. Fresh read-only R art dispatched immediately with raw proof and contact-sheet path. Contract patch round2 merged 244c715 and fresh R contract-final dispatched.

## 2026-09-12 import and simulation proof progress
Godot 4.7.2 imported all 15 GLBs successfully. This intermediate editor import reports missing scenes/Main.tscn (client lane not yet merged), so it is not a runtime pass. Raw intermediate output retained as proof/godot-art-import.txt. Lead owns generated .glb.import and shared-script .uid metadata on main; tests/.gdignore excludes .NET proof source from Godot import/export. Sim reports both frozen seam and supplemental timing proof green with real factory; substantive regression/full match proof still in progress.

## 2026-09-12 approvals
Fresh R contract-final APPROVE at pre-implementation gate: source pinning, FileAccess, supplemental invocation and Build timing addressed. Historical red is correctly not current runtime proof. R art APPROVE for art range: distinct original geometry, catalog mappings and lane boundaries conform; 15/15 Blender imports. Runtime consumption/team tint/rotors remain integration checks. Native .blend reopen/equivalence is a nonblocking proof gap; GLB runtime import is authoritative for this slice. Both full verdicts retained in reviews/.

## 2026-09-12 client build progress
Client reports pinned Godot.NET.Sdk/4.7.2 build with zero warnings/errors. Public-order proof pilot supports --proof-play, --proof-output=<path>, --proof-quit and uniform --proof-speed=N; no sim-internal mutation. Actual integrated match remains pending. Lead launch wrappers pass shell syntax and reviewer harness parses; git diff --check clean.
