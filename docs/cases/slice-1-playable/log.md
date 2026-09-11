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
