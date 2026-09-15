# Surprise — verifiable-slice-refactor

## What we thought

The slice already had a green full match. The job was evidence: atomics, real GUI, traces, locomotion, fail-closed verify.

## What was true

- Full match was green **and** audit still reported stuck=12 oscillate=42. Those are still diagnostics, not a match fail (Q5).
- `ter.unbuildable` was a real catalog defect in Pathing. Repairing it changed the full-match hash and defeat tick (2033 → 2150).
- Instant tracers were Activity+cooldown. CombatTrace on the snapshot fixed the contract; client now draws only traces.
- ProofPilot still exists; GUI proof is PushInput. Three Godot scenarios pass.
- GLBs did not need a rebuild. Rotors/bob are runtime.
- `review_case.py` process exit was not a verdict. Self-check now fails closed. Codex-specific write stripping remains; this case used Grok `code-reviewer` with no write tools.
- Architect wanted a MatchClient/Battlefield split. Lead kept one client SCC. That was faster and matched the import graph.

## Defaults taken (accepted at close)

Q1–Q10 as spec. TerrainAt OOB is `ter.block`. Ghost occupancy still blocks visible units. User did not overrule.

## Spec defects

- `acceptance-matrix.md` “known gap” column still describes the baseline tree in places; current evidence is in `proof/` and `reviews/`.
- AGENTS.md still has an old sentence that `review_case.py` targets `slice-1-playable`; the command table is updated.
- `tests/Program.cs` leftover comment about missing i-sim atomics; `tools/verify.sh` is the official selector.

## Follow-ons (not this case)

- Guard/sell GUI scenarios
- Fog-hidden pick
- Oscillate / brief-block-then-free atomics
- Fog-on CombatTrace atomic
- Golden-frame harness (user-owned)
- Hosted CI if desired later
