# verifiable-slice-refactor

Frozen ask, 2026-09-14. Later restates append; do not rewrite this block.

Make the existing Godot slice-1 client a game that agents can keep evolving with reproducible results. Movement, targeting, damage, input, presentation, and interactions between systems must have concrete acceptance contracts and executable checks. A future agent should be able to explain what a change affects, reproduce a failure, repair it, and demonstrate that the repaired behavior and relevant surrounding behavior work.

Do not promise universal correctness. Establish strong, explicitly bounded evidence for the implemented game. Make uncertainty visible and actionable.

This is a new case. Do not reopen closed `slice-1-playable`. The existing Godot application is the application to improve. No second app. No engine migration. No future catalog (Forge/Veil, specialists, LAN, lobby, backend, commander naming, final balance).

User authorized this prompt as accept of outcome, limits, and defaults. Visual/play acceptance and final closure remain user decisions. No push. Stop at `ready-to-close` only when required engineering gates and independent review hold.

## Outcome in scope

- Inspect the whole current implemented slice and improve every area needed for the outcome (sim, orders, AI, pathing, combat, economy, production, construction, ownership, fog, client input, selection, camera, HUD, visual feedback, assets, locomotion presentation, audio timing where affected, diagnostics, tests, launch/build tooling, agent workflow).
- Keep good code and working seams. Structural change only when it improves correctness, isolation, reproducibility, or maintainability, with evidence.
- Build missing verification and development controls.
- Correct implementation defects against the existing catalog.
- Add minimal original locomotion presentation for current infantry and mobile vehicle roles using the existing Godot/Blender/glTF pipeline. Write specific animation requirements before building. Preserve current visual direction; significant new art direction remains a user decision at final review.
- Finite acceptance matrix for the CURRENT slice. Independently authored executable contracts. Atomic scenarios, one verification entry point, structured outcomes, replayable evidence.
- Real client input path, not only helper tests. Render captures. Negative-control evidence. Generalized restricted review tooling.
- Preserve 8-slot match model, placeholder tuning file, catalog ids, tick sim authority, Godot presentation.

## Explicit non-goals

Future catalog, second application, replacement engine, imported third-party game IP, push, hosted CI as the acceptance path, rewriting every file, claiming cross-GPU pixel identity or universal correctness.
