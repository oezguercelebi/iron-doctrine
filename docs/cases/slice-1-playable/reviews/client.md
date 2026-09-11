**BLOCK**

- **P2 — `src/Client/MatchClient.cs:275`: Ordinary Attack becomes inaccessible for Rifle units against buildings after capture research.** Right-click always submits Capture, including against occupied enemy garrisons, which the frozen contract explicitly excludes from capture. No explicit Attack mode provides an alternative.

- **P2 — `src/Client/FieldHud.cs:238`, `src/Client/MatchClient.cs:338`: Cancellation can target the wrong producer.** The displayed queue belongs to the first selected entity in snapshot order; cancellation targets the lowest owned ID. With multiple producers selected, these can differ—the contract guarantees no snapshot ordering.

- **P2 — `src/Client/Battlefield.cs:239`: Enemy combat rendering depends on private order data.** Instant-fire tracers require `TargetId`, which conforming enemy snapshots must redact. Enemy Rifle/Tank/Scout fire consequently loses its tracers; facing also depends on private `Destination`/`TargetId` at line 350.

- **P2 — `src/Client/FieldHud.cs:178`: Selection bypasses radar shutdown.** Selected units retain live minimap blips when `Player.Radar` is false, undermining the required shutdown after low power or command loss.

**Proof gaps:** The supplied log establishes disposable-fixture compilation only. Integrated Godot 4.7.2 import/boot, a completed live match, controls, and rematch remain unproven. The prior image supplies visual context only; `ProofPilot` directly submits orders and cannot establish mouse/HUD input correctness.