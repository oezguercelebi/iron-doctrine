**BLOCK** — client/scenes range only.

- **P2 — `src/Client/MatchClient.cs:378`: Build can use a different position from its legal ghost.** `ApplyMode` forwards the picked entity ID. Simulation execution (`src/Sim/Orders.cs:125`) then replaces the approved position with that entity’s position. Clicking within a nearby unit’s selection radius can therefore move construction elsewhere or reject an otherwise legal placement. The proof pilot submits Build without this target ID and misses this path.

- **P2 — `src/Client/FieldHud.cs:220`: Enemy garrisons and transports falsely appear empty.** Enemy `OccupantIds` are deliberately redacted (`src/Sim/Intel.cs:41`), but the HUD displays their empty array as an actual passenger count. An occupied enemy garrison consequently shows “Passengers 0/8,” giving incorrect tactical information. Enemy activity is similarly displayed as “IDLE” from a redacted default at line 213.

**Proof gaps:** Supplied output reports successful R1 compilation; the inspected screenshot confirms a rendered victory screen. Manual controls, rematch, and runtime verification of the R1 interactions remain unproved. No tests were executed during this review.