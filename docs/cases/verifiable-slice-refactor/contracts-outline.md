# Contracts outline — expectations from catalog

C turns these into executable acceptance tests. **Expectations come from the catalog**, not from current green tests. If code/tests disagree with catalog, fix code or obtain an authorized catalog amendment — do not bless current behavior.

Each block: Rule · Scenario · Assertions · Proof. Scenario ids match `acceptance-matrix.md`.

---

## path.ground_traverses_unbuildable

- **Rule:** `TERRAIN.md` `ter.unbuildable` — ground yes, air yes, build no. Owning claim: ground movers traverse; `CanPlace` still denies build.
- **Scenario:** Slice map cell tagged `ter.unbuildable` (rocks/decor rect). Ground unit (`build.dozer` or `inf.rifle`) ordered to Move across/onto that cell. Separate: same cell Build `power.fusion` via CanPlace/Build.
- **Assertions:** Move path accepted and unit occupies/traverses unbuildable ground within bounded ticks **or** documents a non-terrain blocker (unit occupancy). Build/CanPlace **rejected** with terrain/build reason. Air still ignores ground clutter. Forbidden: treating unbuildable as ground-impassable.
- **Proof:** Atomic under `tools/verify.sh`; SeamConformance red until fixed; sim layer. Do not rewrite TERRAIN.md.

## build.ghost_matches_arrival_occupancy

- **Rule:** MECHANICS§3 construction; CanPlace must not fog-leak; provisional slice-1: CanPlace blocks **visible** units.
- **Scenario:** Legal empty ghost point vs point occupied by visible own/enemy unit vs remembered-fog building footprint.
- **Assertions:** Empty legal → Allowed. Visible unit overlap → blocked (provisional). Hidden enemy unit must not be revealable via CanPlace reason codes. Execute Build uses approved ghost point, not picker entity position.
- **Proof:** Sim atomic + client build-ghost scenario. Mark provisional in evidence.

## path.distinguish_wait_vs_stuck

- **Rule:** Movement must distinguish legitimate waiting (dock queue, blocked by allied body, ordered Wait/Loading) from stuck/oscillation failure modes.
- **Scenario:** (1) Gatherer waiting for exclusive dock loader. (2) Unit oscillating between two cells with no progress. (3) Unit blocked briefly then freed.
- **Assertions:** (1) classified wait, not stuck gate failure. (2) stuck/oscillate diagnostic fires. (3) no persistent stuck. FullMatchProof **must not** fail solely on pre-calibration heuristic counts.
- **Proof:** BehaviorAudit + calibrated criteria; verify may report; gate only after thresholds accepted. Audit layer ≠ automatic match failure (Q5).

## combat.tank_cannot_hit_chinook / combat.rocket_missile_kills_chinook

- **Rule:** DAMAGE cannon vs air = none; rocket vs air = good. SLICE: patriot/rocket kill chinook; tanks must not. DELIVERY missile for rockets/patriot.
- **Scenario:** `armor.basic` Attack `eco.chinook` in range. `inf.rocket` (or `def.patriot`) Attack chinook.
- **Assertions:** Tank order rejected or no HP damage / no air-effective shot. Rocket/patriot: `del.missile` projectile appears before impact; chinook HP decreases; kill possible. Forbidden: tank damaging air.
- **Proof:** Sim atomics; pair with `combat.missile_exists_before_impact`.

## combat.no_duplicate_missile_impact

- **Rule:** DELIVERY — one flight object resolves once.
- **Scenario:** Single missile shot vs target; observe until impact or cancel.
- **Assertions:** Exactly one damage application for that shot id. Negative control: fault injection or twin-resolve attempt must be caught by the check in an isolated worktree.
- **Proof:** Atomic + negative-control worktree (lead/integrate).

## combat.instant_tracer_tied_to_shot

- **Rule:** Presentation claim for instant combat feedback requires correlation to a resolved sim shot/hit. Causal ids are diagnostic fields on existing snapshots/events (Q4), not new catalog rows.
- **Scenario:** Rifle/`del.instant` attack producing a hit; and a tick where Activity is Attacking but no resolved shot this tick.
- **Assertions:** Tracer/FX spawns only when correlated shot/hit id present. No FX from Activity+cooldown alone. Missiles already keyed by `ProjectileSnapshot.Id`.
- **Proof:** Client/presentation layer; contract fields frozen by C; injected or controlled client scenario. Render frames optional support, not cross-GPU identity.

## match.elim_by_buildings_only + verify.sealed_replay

- **Rule:** INVARIANTS — win by enemy buildings gone; army wipe recoverable. Determinism: same sealed inputs ⇒ same StateHash.
- **Scenario:** Default human-v-AI public-order FullMatch remains. Additive: record sealed order log from a finished match; replay in fresh process.
- **Assertions:** Public-order match still finishes with stable hash class of evidence. Sealed replay reproduces hash; on deliberate divergence, report first divergent tick (field-level when API supports it). Forbidden: replacing SimProof with `dotnet test`.
- **Proof:** `tools/proof.sh` + `tools/verify.sh` replay scenario; replay package artifact.

## intel.enemy_queue_redacted + client.no_fog_world_leak

- **Rule:** SIGHT fog; snapshot redaction; no fog leak via HUD/world picking.
- **Scenario:** Viewer fog; enemy producer with queue; client pick/hover/minimap paths.
- **Assertions:** Snapshot omits/redacts enemy queue contents. Client helpers and real input path cannot read fog-hidden entity private fields. Radar≠LOS.
- **Proof:** Sim intel atomic + client leak tests (helper insufficient alone for GUI claim).

## client.input_build_ghost_point

- **Rule:** HUD `win.place` + MECHANICS Build — production input dispatch.
- **Scenario:** Development control surface injects InputEvents through MatchClient production `_Input` (not ProofPilot.Send bypass) to select dozer, enter build, place ghost.
- **Assertions:** Submitted `MatchOrder` carries CanPlace-approved point; sim constructs there. ProofPilot path is not evidence for this row.
- **Proof:** Client input layer under verify.sh / Godot-backed harness named in evidence. `src/Client/Proof/run.sh` remains helper-only.

## presentation.locomotion_root_motion / presentation.chinook_rotor_spin

- **Rule:** See `locomotion-requirements.md`. Preserve visual direction; no skeletal walk cycles.
- **Scenario:** Moving `inf.rifle` / `armor.basic` / `build.dozer`; alive `eco.chinook`; idle structure.
- **Assertions:** Movers: root translation interpolates toward sim pose; yaw faces move/aim per requirements. Chinook: `rotor_left`/`rotor_right` spin while operating. Structures: no locomotion bob. Forbidden: armature walk-cycle dependency.
- **Proof:** Presentation layer; asset verify allowlist; user visual/play acceptance at close.

## verify.entry_fail_closed / verify.review_case_generalized

- **Rule:** AGENTS verification layers + workflow. Missing required check ≠ pass.
- **Scenario:** `tools/verify.sh --scenario <missing>` or required artifact absent; `review_case.py` with wrong case / empty proof.
- **Assertions:** Nonzero exit. review_case parses APPROVE/BLOCK from reviewer output; process success alone is not approval; parameterized `verifiable-slice-refactor`.
- **Proof:** Tooling checks in i-verify; lead uses generalized review for R.

---

## Additional atomics (compact)

Same field shape; C expands to executable stubs. Catalog owners as named:

| scenario | Rule (short) | Critical assertion |
| --- | --- | --- |
| match.boot_command_dozer | MATCH_AI start_units | Command+dozer present at tick 0 for filled slots |
| match.pause_freezes_clock | local pause | Tick frozen while paused |
| match.resign_is_loss | INVARIANTS | Resign ⇒ eliminated/loss |
| match.army_wipe_recoverable | INVARIANTS | No buildings⇒defeat; units-only wipe≠defeat |
| build.prereq_blocks_factory | TECH_TREE | Queue/Build factory denied without prereq |
| build.incomplete_lost_on_builder_death | INVARIANTS | Scaffold gone when builder dies |
| build.facing_sets_default_rally | MECHANICS§3 | Facing sets rally |
| eco.dock_exclusive_loader | MECHANICS§4 | Second gatherer waits/does not co-load |
| eco.return_own_dropoff_only | INVARIANTS | No ally drop-off return |
| eco.free_chinook_once_on_build_not_capture | eco rules | Free chinook once on build only |
| eco.sell_refund_cut | ab.sell | Sell refunds cut; building gone |
| power.brownout_stops_radar_and_patriot | §5 / sight | Low power: radar false; patriot unpowered |
| prod.queue_cancel_refund | §6 | Cancel refunds paid cost |
| prod.rally_followed | §6 | Unit moves toward rally |
| combat.rifle_beats_rocket_even_fight | DAMAGE | Even fight outcome favors rifle |
| combat.rockets_counter_tank | DAMAGE | Rockets effective vs tank |
| combat.crush_ff_on_allied_infantry | crush/TEAMS | FF crush per team rules |
| combat.force_attack_friendly | ForceAttack | Accepted and damages per rules |
| garrison.enter_flips_neutral_owner | §8 | Neutral box flips on enter |
| garrison.small_arms_poor | DAMAGE | Poor damage vs garrison armor |
| garrison.destroy_spills_hurt | §8 / SLICE | Spill on destroy; no clear tool |
| transport.scout_passengers_fire | §14 | Scout fire-out |
| transport.chinook_passengers_silent | §14 | No chinook fire-out |
| transport.exit_local_reach | §14 | Remote unload rejected/clamped |
| capture.channel_interruptible | §10 | Interrupt resets/stops channel |
| capture.clears_queue_refunds_prior_owner | §10 | Prior owner refunds/clears |
| repair.dozer_pays_allied_building | §15 | Paid repair raises HP |
| repair.factory_own_vehicles_only | §15 | Enemy vehicle not factory-repaired |
| vet.ranks_and_heroic_self_heal | §9 | Rank thresholds; self-heal elite+ |
| intel.radar_not_los | SIGHT | Radar blips ≠ camera LOS reveal |
| ai.no_hidden_army_cheat | MATCH_AI | AI decisions fog-symmetric |
| ai.jobs_base_gather_compose_attack_defend | jobs | Jobs observed in full match |
| client.guard_keeps_unit_target | Guard | Guard target retained via real input |
| client.sell_mode_then_click | win.sell | Sell via HUD mode + click |
| assets.no_skeletal_walk | locomotion-requirements | verify_assets rejects armatures/skeletal walks |

Fixture setup may arrange state; after measurement starts, use production order/combat paths — no privileged damage teleport as proof of the action under test.
