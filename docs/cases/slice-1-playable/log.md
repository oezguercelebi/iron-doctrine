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

## 2026-09-12 client merge and R
Client 3f79a50 passed lane check (7 paths src/Client/** and scenes/**). Merged on main; fresh R client dispatched with raw standalone compile proof and exact contracts/data/root context. Lead synchronized disposable smoke fixture to merged client source and rebuilt: zero warnings/errors. Repeating its startup from main cwd hit Godot C# script-path/class discovery for the disposable Main, so this repeat is not counted as a runtime pass; child diagnosing fixture commands. Its earlier screenshot is visual context only, not final integrated-match evidence. Sim source still in worktree completing full paired matches.

## 2026-09-12 standalone fixture resolved; match progress
Client child confirmed Godot script-path generation was sensitive to /tmp versus canonical /private/tmp plus build cwd. Rebuilt from inside fixture, then headless and visible boots both exit0: CLIENT_READY slot=0 entities=19 models=15, CLIENT_SMOKE_BOOT_OK. Raw corrected logs copied to proof/client-build.txt and client-smoke-{headless,visible}.txt; initial error retained separately. No product patch needed; root run wrapper already changes to project root before build. Sim complete default match reached actual defeat near tick1900 with identical paired replay; proof caught occupied-destination pathfinding and AI composition cursor bugs, now corrected and rerunning before merge.

## 2026-09-12 simulation merge and main proof
Sim 5588be7 passed lane check: 10 source/test paths; contracts, data, frozen SeamConformance and test manifest unchanged. Merged on main. ./tools/proof.sh passes seam, next-tick submission, 14 mechanic groups and full default human-v-AI match: 17 public player orders, defeat at tick2217, paired full state hash AE566419C67AD2C9BB97AFF50D73A4E716DCA74AB629A3F33F6EC1A2B49706D1; full match portion6.5s. Raw output proof/sim-main.txt. Fresh R sim dispatched immediately with source and raw main proof. Integrated Godot C# build and boot now running.

## 2026-09-12 main build/boot and R client block
Integrated Godot build succeeds with zero warnings/errors; import has no errors/warnings; headless boot reports CLIENT_READY slot=0 entities=2 models=3 (only initial visible entities, model cache loads on demand). Main simulation proof remains green. Client R blocks P2: no normal Attack alternative after rifle capture research; cancellation producer differs from displayed queue under multi-selection; enemy tracers/facing depend on redacted TargetId/Destination; selected minimap blips bypass radar-off. Assigned client patch round1 in fresh isolated worktree from main. Full verdict preserved. These are in-scope fixes, no accepted contract change.

## 2026-09-12 first complete visible match
Actual integrated Godot/OpenGL local match, same default setup, ordinary player-order proof pilot, uniformly accelerated4x clock. Result: PROOF_PLAY_FINISHED tick=3630 result=victory orders=149 models=15. Actual viewport saved successfully in proof/first-finished-match.png and inspected: VICTORY, all opposing buildings eliminated, 03:01 elapsed. No engine/runtime errors. Raw log proof/first-visible-match.txt. This is a real sim/view match, not the earlier static fixture. Remaining client R fixes and manual inputs/final review still required; case not ready-to-close yet.

## 2026-09-12 R sim block and client patch1 merge
Sim R blocks five executable edge cases: builder embedded in footprint; fallback route treated as arrival outside range; owned occupied garrison ignored by elimination; turret attack/force-attack ignored; stopped final cargo cannot resume after docks deplete. Patch round1 assigned to sim in fresh isolated worktree from 1b35a9b. Existing outcome and contract hold. Client patch1 5df3b25 passed lane check (src/Client only), merged bb5c572. Build zero warnings/errors. Fresh R client-r1 dispatched immediately on merged source, with first visible victory proof.

## 2026-09-12 manual controls proof
Actual patched client against integrated sim, Computer Use mouse and keyboard. Fusion placed on open ground, completion changed funds6000→5400 and power0→20; multiple Dozers queued; control group1 saved/recalled. Production cancellation independently observed with queued batch: funds2500→3000 after next tick and queue item removed. Box selection selected7units, rightclickMove activity displayed, wheel zoom and H guide worked. Escape paused clock; Resign from pause produced DEFEAT at01:12; Rematch reset to00:00,6000funds, Command+Dozer. Screenshots and detailed limits in proof/manual-input.md. Quit to desktop exited cleanly. These checks supplement simulation/order proofs; they do not claim every combat interaction was manually clicked.

## 2026-09-12 R client-r1 block — patch round2
Fresh R found two correctness issues: P2 MatchClient:378 Build leaked picked target id so execution could substitute nearby entity position for ghost point; P2 FieldHud:220/213 redacted enemy passenger/activity data displayed as known empty/idle. These are correctness fixes, not nits; assigned second and final client patch round in a fresh worktree. No frozen seam or outcome change. Manual controls evidence is now recorded; original R did not have that newly recorded evidence.

## 2026-09-12 sim patch1 merged and R dispatched
Sim d935dc3 lane check passes: five src/Sim files, Program invocation and new ReviewRegressionProof only. Merged a47ae35; full main proof passes, including all five R regression scenarios. Full paired match defeats player at2192, hash C22B028182E524DEE442193CE933D104F4988477B59CFAB43F429AEF3141AB73. Fresh R sim-r1 dispatched with raw main proof; frozen tests/contracts/manifests unchanged. Worktree removed after merge.

## 2026-09-12 evidence import hygiene
Client patch2 import uncovered that Computer Use screenshot files returned JPEG bytes despite their .png filenames. Lead corrected only the six manual screenshot extensions and references to .jpg, and excluded docs/ from Godot resource import with .gdignore. No image contents or gameplay changed. Final import will verify the correction.

## 2026-09-12 final client patch merged, sim second review block
Client 5eb37db passed exact lane check, merged4842901. Main targeted input proof and Godot build pass: clicked ground point survives incidental picked entity; private enemy intel unknown; zero build warnings/errors. Fresh client-r2 R dispatched on e5f29ee.
Sim R1 returned P1 hiddenTargetId position substitution and P2 appended Exit executes immediately / captured defense retains old owner orders. Assigned second and final sim patch round in fresh worktree. The client now strips incidental targets, but sim must independently enforce the public information and action-queue contract. Full R verdict retained; in-lane correctness, no spec amendment.

## 2026-09-12 client approved
Fresh client-r2 R APPROVE: both R2 findings resolved, raw targeted checks and main build pass. It correctly notes post-R2 visible match remains integration work and manual dispatch covers only the recorded subset. Main post-R2 import and headless startup already pass without errors; final visible completion follows sim patch2.

## 2026-09-12 sim second patch baseline evidence
Raw proof/sim-r2-baseline.txt records an actual temporary build of e5f29ee sim source with the new boundary regressions. Existing proofs pass; all three new groups fail (hidden incidental target, early appended exit, retained capture orders), process exit134. This is intentional regression evidence, not a current integration failure.

## 2026-09-12 final simulation merge and integrated gates
Sim08feb3b passed lane check: three src/Sim files plus Program and OrderBoundaryProof only. Merged90b2864. Main full regression exits0, including all8review regression groups; paired full match hash unchanged at tick2192. Fresh sim-r2 R dispatched immediately with raw main proof and baseline failures. Godot .NET final build zero warnings/errors, final import and headless boot clean. Visible final match launched from the merged source with ordinary public player orders and uniform4xclock. Worktree removed after merge; no implementation lanes remain open on disk.

## 2026-09-12 final visible match passed
Merged90b2864 actual Godot4.7.2/OpenGL match reached VICTORY at tick2701 (02:15),113ordinary public player orders,15models loaded. Engine viewport saved proof/final-finished-match.png successfully; lead inspected full rendered result. Raw final-visible-match.txt, exit0, no runtime errors. Post-patch client input proof was also repeated on final sim and passed. Mac later locked, so a further Computer Use observation was unavailable; this final screenshot comes directly from the game's viewport. Earlier manual mouse/keyboard evidence remains explicitly scoped to its tested head. No OS-lock bypass attempted or needed for engine-rendered proof.

## 2026-09-12 final case R dispatched
Fresh independent case-final R dispatched over accepted baseline63a008f..f0a0285, all merged source, interaction/spec drift only. Complete current regression/build/import/headless/visible proof, original GLB validation, scoped manual input and catalog system context supplied. Sim-r2 R runs independently on the same final sim source; ready-to-close requires both approvals. No implementation worktrees remain.

## 2026-09-12 final reviews block; patch cap reached
Fresh sim-r2 R BLOCK P2 Systems.cs:228: nearby unload anchor falls through an unbounded FindFree search, allowing passengers outside local unload reach on default blocked terrain. Fresh case-final R BLOCK P2 CommandIntent.cs:12: Guard target id is stripped, so G+moving friendly unit guards a fixed point; the client proof currently asserts that wrong behavior. Both full verdicts are retained. Final case R inspected the02:15victory screenshot and passing final proof logs. The sim R was dispatched before final visible completion and accurately lacked that later artifact.
Two sim patch rounds are exhausted. Crew rule: "budget.patch-rounds times, then stop for the user with both verdicts quoted." No third implementation round was started. Case is idle-user, not ready-to-close. Request one additional patch round to repair both remaining accepted-behavior findings, then rerun targeted/main/visible proof and fresh independent reviews. Same case, same outcome, no push. All worktrees are merged and removed.
