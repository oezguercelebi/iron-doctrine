# slice-1-playable
Outcome: the accepted intent in intent.md, a playable, finishable local 1vAI match on Godot 4.7.2 .NET with original Blender glTF assets.
Non-goals: all exclusions in intent.md and SLICE.md.
Lanes: contract proposal (precedes simulation/client); art assets independent; simulation + proofs; Godot presentation/input/AI driver if allocated by frozen contract. Hubs, project manifests, shared contract and integration belong to lead on main.
Frozen interface: contract.md + src/Contracts/** + tests/SeamConformance.cs. IMatch, IMatchFactory, MatchClient.Initialize(IMatch, Func<IMatch>). data/slice1.placeholders.json is the sole gameplay tuning file; lead owns it.
Proof: pure C# executable regression scenarios, deterministic replay, full order-driven match to victory/defeat; Godot .NET build/import/headless boot; visible local play to an end screen; glTF import and controls inspection; independent read-only review on each merge and final integration.
Defaults: use 4.7.2 stable; no optional patriot link; provisional working role labels, procedural original Blender hard-surface art; no new dependencies beyond authorized Godot/.NET/Blender tools.
Budget: patch-rounds: 2; deviations: in-lane; unanswered: default; push: at-close; close: user; device: at-close.

## 2026-09-14 amend — playable feel

User authorized a third implementation wave after the patch-cap stop. Outcome unchanged: finishable local 1vAI match. Same frozen contract/test.

Must now also hold:
- Selected own producer shows a **persistent rally marker** at `RallyPoint` (re-select still shows it). Rally verb only on production buildings.
- Guard with a clicked unit **follows** that unit (sim already can). Ground Guard remains a point.
- Unload stays within local reach of the carrier; no map-wide `FindFree`.
- Build ghost legality matches arrival (`GroundFits` including units). Green ghost must place.
- Clicking an enemy does not blank the command bar for the owned part of the selection.
- Pick prefers infantry/mobile over a larger building when the click is on both.
- Edge pan works at the screen edge (including over HUD).
- Sell is a cursor/mode, then click; not instant Delete.
- `sfx.rally` and `sfx.place` play (beeps fine).

Non-goals still: LAN, Forge/Veil, netcode, catalog number pass, new prefixes.
