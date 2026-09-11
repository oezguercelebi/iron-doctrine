**BLOCK**

- **P2 — `tools/review_case.py:34`:** Numbered source comes from the working tree, while the diff uses the requested commits. A dirty checkout or different checked-out revision gives the tool-free reviewer conflicting code and incorrect line references, undermining the range-specific review gate.

- **Proof gap — `tests/SeamConformance.cs:33`:** The frozen test never checks gameplay state between `Submit` and the first `Step`. Immediate construction/debit during submission can pass its eventual-completion assertions, leaving the promised next-tick semantics unverified (`docs/cases/slice-1-playable/contract.md:19`).

The supplied CS0234 is the authorized missing-factory red, not a blocker. It establishes no runtime assertions; sim/client and finished-match proofs remain pending. No additional accepted-intent drift was identified in the supplied seam/data.