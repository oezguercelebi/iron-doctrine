# Acceptance evidence

Implementation and manual proof are complete; post-patch integrated run and fresh final case R remain pending. Historical failed proofs are retained as labeled intermediate evidence, not counted as current passes.

| Requirement | Evidence |
| --- | --- |
| Eight-slot/team/loadout model; two filled | Frozen SeamConformance; MechanicsProof eight-slot teams; proof/scope-check.txt |
| Single tick/order list; detached deterministic state | Plain .NET executable; SubmissionTiming; paired full state hashes including AI/fog/pending orders |
| Command+Dozer boot; army wipe recoverable; elimination/resign | Seam and elimination regressions; first visible victory; manual-resign.png |
| Dozer building, legal placement, builder death loses incomplete | Construction proof; R1 embedded-builder/blocked-route regressions; mouse Fusion construction |
| Finite docks, exclusive loader, owner-only return, free first gatherer | Economy and R1 stranded-final-cargo regressions; visible match gathering |
| Low power disables defenses/radar; radar never grants vision | Power/intel proof; reviewed HUD gating |
| Production/prerequisites/capture research/refund/rally | Production/capture proof; manual queue and500fund refund screenshot |
| Counters, air exclusion, world missiles, friendly fire/crush | Combat fixtures; world projectile rendering in complete visible match |
| Garrison entry/exit/armor/spill; small arms poor | MechanicsProof; R1 owned-garrison survival regression; reviewed client input mappings |
| Scout holds5 and fires out; Gatherer transport/local unload | Transport regression; reviewed entry/exit input mappings |
| Veterancy, XP, buffs and self-heal | Veterancy regression; snapshot-driven HUD |
| Interruptible capture, transfer, no free gatherer on capture | Capture regression; R1 blocked-route interaction tests |
| Dozer building repair and factory vehicle repair | Repair regression; R1 remote-repair rejection |
| Fair AI base/gather/compose/attack/defend | Hidden-information equality/depleted-dock tests; default full paired matches |
| Requested orders, selection and controls | src/Client/README.md; reviewed dispatch; manual-input.md for directly exercised GUI subset |
| Original mirrored map with close/contest docks and clutter | Single map data; pathing/placement tests; rendered view |
| Real original Blender/glTF bodies | art-import.txt:15/15GLBs,100785triangles; Godot imports; runtime loads15models; assets manifest/provenance |
| Placeholder values centralized; excluded scope absent | Single labeled JSON, scope-check.txt; independent lane/final source review |

## Integration gates

1. Plain .NET regression executable: **PASS**, proof/sim-r1.txt, including five R regressions.
2. Complete deterministic replay: **PASS**, default match defeats player tick2192; paired hash C22B028182E524DEE442193CE933D104F4988477B59CFAB43F429AEF3141AB73.
3. Godot4.7.2 .NET compile: prior integrated builds **PASS**, zero warnings/errors; final post-patch build pending.
4. Original glTF validation/import: **PASS**, proof/art-import.txt and godot-import.txt.
5. Headless Godot startup: prior integrated boot **PASS**; final post-patch startup pending.
6. Visible complete local match: **PASS** at tick3630, victory,149publicorders,15models; first-visible-match.txt/png. Mouse/keyboard construction, queues/refund, control group/box selection, move/zoom, pause, resign and rematch **PASS**, manual-input.md and screenshots. Final post-patch finish pending.
7. Lane/scope hygiene and fresh case R: mechanical checks **PASS**; lane reviews and final review pending completion.

Crew's TypeScript example is inapplicable to the locked C# stack. No npm project was added. Full integration checks use the stack equivalents above.

## Limits

These are placeholder balance values and a small original teaching map. Optional AA link is skipped. The complete-match driver sends ordinary player orders from public snapshots; it does not click each HUD command. Manual GUI proof covers the subset explicitly listed above; specialized commands also have simulation regressions and independently reviewed dispatch. Native Blender source roundtrip equivalence is not separately proved; exported glTF import and runtime consumption are proved. No network play, excluded factions, or backend is included.
