**BLOCK** — simulation/tests lane.

- **P1 — `src/Sim/Orders.cs:126`: Position orders expose hidden target positions.** Move validation accepts an unseen enemy’s `TargetId`, then execution copies its current position into the owned actor’s public `Destination`. The same substitution makes client Build orders use a different position from the validated placement ghost.

- **P2 — `src/Sim/Orders.cs:123`: Appended Exit ignores queued movement.** Exit executes immediately regardless of `Append`. Shift-queuing movement followed by unloading therefore unloads passengers at the departure point, violating the frozen action-queue contract. Existing transport tests do not cover this sequence.

- **P2 — `src/Sim/Systems.cs:193`: Capture retains the previous owner’s combat orders.** Ownership changes without clearing `building.Actions`. A captured, powered Patriot previously force-attacking the captor’s building continues damaging that now-friendly building. The capture proof covers production refunds but misses this retained control.

**Proof gaps:** Supplied R1 output reports all regressions and deterministic defeat passing. Integrated Godot and visible/manual evidence predates the simulation fixes; verification of the reviewed head remains outstanding. No tests were executed during this review.