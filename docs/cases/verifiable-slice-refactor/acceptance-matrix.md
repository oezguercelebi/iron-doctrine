# Acceptance matrix — current slice-1

Scenario ids are **verification names**, not catalog prefixes. Catalog wins if tests disagree. Baseline tree: `c5aefdd`.

Legend for **known gap**: `bug` = confirmed catalog defect · `audit` = diagnostic finding, not yet a confirmed bug · `missing` = no executable evidence · `provisional` = accepted temporary rule · `ok` = existing evidence aligned (may still need atomics).

| scenario id | catalog rule | claim | current boundary | current evidence | required new evidence / layers | known gap | owning plan row |
| --- | --- | --- | --- | --- | --- | --- | --- |
| match.boot_command_dozer | MECHANICS§1, MATCH_AI start_units, SLICE | Match boots with command + dozer per filled slot | `Match` / setup | Seam + FullMatch | Atomic + verify.sh select | ok→atomic | i-sim, i-verify |
| match.pause_freezes_clock | MECHANICS§1, HUD pause | Local pause freezes sim clock; orders do not advance gameplay | `IMatch.SetPaused` | Seam | Atomic assertions on tick | ok→atomic | i-sim |
| match.resign_is_loss | INVARIANTS win/lose | Resign ⇒ loss for that slot | Orders/Outcome | Mechanics Outcome | Named atomic | ok→atomic | i-sim |
| match.army_wipe_recoverable | INVARIANTS | Army wipe alone is not defeat if buildings remain | Outcome | Mechanics Outcome | Atomic negative | ok→atomic | i-sim |
| match.elim_by_buildings_only | SLICE / INVARIANTS | Win by destroying enemy buildings | FullMatchProof | Full match defeat tick 2033 | Keep + sealed replay twin | missing sealed replay | i-sim, i-verify |
| match.eight_slot_model | INVARIANTS, TEAMS | 8-slot model preserved; unused closed; no delete of slot/team/loadout ids | Setup / Seam | Mechanics Outcome | Seam regression must keep ids | ok | c-freeze, i-sim |
| build.prereq_blocks_factory | TECH_TREE slice, MECHANICS§3/6 | Factory blocked without prereqs | Systems/Orders | Mechanics Construction | Atomic | ok→atomic | i-sim |
| build.incomplete_lost_on_builder_death | INVARIANTS | Incomplete building lost if builder dies | Systems | Mechanics Construction | Atomic | ok→atomic | i-sim |
| build.ghost_matches_arrival_occupancy | MECHANICS§3 | Place uses legal ghost point; build occupancy consistent | CanPlace vs GroundFits | ClientInputProof + regressions | Atomic ghost vs execute | provisional visible-unit CanPlace | i-sim, Q2 |
| build.facing_sets_default_rally | MECHANICS§3 | Facing on place sets default rally | Orders/Systems | ReviewRegression | Atomic | ok→atomic | i-sim |
| build.unbuildable_blocks_place | TERRAIN `ter.unbuildable` build=no | Cannot place on unbuildable | CanPlace/TerrainFits | Partial via terrain pathing tests | Explicit place-deny atomic | ok / tighten | i-sim |
| path.ground_traverses_unbuildable | TERRAIN ground=yes | Ground units may traverse `ter.unbuildable`; build still forbidden | `Pathing.TerrainFits` + GameConfig comment | Mechanics Movement (may encode wrong rule) | Atomic traverse + place-deny; Seam red→green | **bug** (treated impassable) | c-freeze, i-sim, Q1 |
| path.air_ignores_ground_clutter | TERRAIN air, DELIVERY air | Air ignores ground path obstacles except map block | Pathing | Mechanics Movement | Atomic | ok→atomic | i-sim |
| path.distinguish_wait_vs_stuck | MECHANICS movement | Legitimate wait ≠ stuck/oscillate alert | BehaviorLog | audit stuck=12 oscillate=42 exit 0 | Calibrated criteria; audit report; **not** full-match fail on raw counts | **audit** | i-sim, Q5 |
| eco.dock_exclusive_loader | MECHANICS§4 | One gatherer loads a dock at a time | Systems | Mechanics Economy | Atomic | ok→atomic | i-sim |
| eco.return_own_dropoff_only | INVARIANTS | Return to own drop-off only | Systems | Mechanics Economy | Atomic | ok→atomic | i-sim |
| eco.free_chinook_once_on_build_not_capture | ROLES / MECHANICS eco | Free chinook once on drop-off build, not on capture | Systems | Mechanics Construction | Atomic | ok→atomic | i-sim |
| eco.sell_refund_cut | ABILITIES `ab.sell`, MECHANICS§2 | Sell own building; refund a cut | Orders | Mechanics / regressions | Atomic | ok→atomic | i-sim |
| power.brownout_stops_radar_and_patriot | MECHANICS§5, SIGHT, STATUSES `st.low_power` | Low power: radar off, defenses off | Systems/Intel | Mechanics Production + ReviewRegression powered defenses | Atomic brownout | ok→atomic | i-sim |
| prod.queue_cancel_refund | MECHANICS§6 | Queue/cancel refunds appropriately | Orders/Systems | Mechanics Production | Atomic | ok→atomic | i-sim |
| prod.rally_followed | MECHANICS§6 | Produced units follow rally | Systems | Mechanics Production / facing rally | Atomic | ok→atomic | i-sim |
| combat.tank_cannot_hit_chinook | DAMAGE cannon vs air=none, SLICE | `armor.basic` cannot hit `eco.chinook` | Combat/Orders | Mechanics Combat | Atomic + negative | ok→atomic | i-sim |
| combat.rocket_missile_kills_chinook | DAMAGE rocket vs air, DELIVERY missile, SLICE | `inf.rocket` / patriot missile can kill chinook | Combat | Mechanics Combat | Atomic | ok→atomic | i-sim |
| combat.missile_exists_before_impact | DELIVERY `del.missile` | Missile exists as world projectile before impact | Shots / Projectiles snapshot | Mechanics Combat | Atomic mid-flight observe | ok→atomic | i-sim |
| combat.no_duplicate_missile_impact | DELIVERY | Single resolution per shot; no duplicate damage | Combat | none dedicated | Negative-control atomic | **missing** | i-sim, integrate-proof |
| combat.rifle_beats_rocket_even_fight | DAMAGE matrix | Rifle beats rocket in even infantry fight | Combat | Mechanics Combat | Atomic | ok→atomic | i-sim |
| combat.rockets_counter_tank | DAMAGE matrix | Rockets counter tanks | Combat | Mechanics Combat | Atomic | ok→atomic | i-sim |
| combat.crush_ff_on_allied_infantry | DAMAGE crush, TEAMS | Crush FF rules on allied infantry hold | Combat | Mechanics FriendlyFire | Atomic | ok→atomic | i-sim |
| combat.force_attack_friendly | MECHANICS§2 ForceAttack | Force-attack can hit friendly/ground per rules | Orders/Combat | Mechanics FriendlyFire | Atomic | ok→atomic | i-sim |
| combat.instant_tracer_tied_to_shot | DELIVERY instant + presentation | Instant FX only if a resolved shot/hit is correlated | Battlefield tracers | Activity+cooldown only today | Client/presentation layer + causal fields | **bug** (uncorrelated) | c-freeze, i-client, Q7 |
| garrison.enter_flips_neutral_owner | MECHANICS§8, SLICE garrison | Enter flips neutral garrison owner | Systems | Mechanics Garrison | Atomic | ok→atomic | i-sim |
| garrison.small_arms_poor | DAMAGE vs garrison poor | Small-arms vs garrison stay poor | Combat | Mechanics Garrison | Atomic | ok→atomic | i-sim |
| garrison.destroy_spills_hurt | MECHANICS§8, SLICE | Destroy building spills occupants hurt; no clear tool invented | Systems | Mechanics Garrison | Atomic | ok→atomic | i-sim |
| transport.scout_passengers_fire | MECHANICS§14, SLICE scout | Scout passengers can fire out | Combat/Systems | Mechanics Transport | Atomic | ok→atomic | i-sim |
| transport.chinook_passengers_silent | MECHANICS§14 / roles | Chinook passengers do not fire out (slice claim) | Systems | Mechanics Transport | Atomic | ok→atomic | i-sim |
| transport.exit_local_reach | MECHANICS§14 | Exit only to local legal reach | Orders/Pathing | Mechanics Transport + ReviewRegression | Atomic | ok→atomic | i-sim |
| capture.channel_interruptible | MECHANICS§10, DELIVERY channel | Capture is channel, interruptible | Systems | Mechanics Capture | Atomic | ok→atomic | i-sim |
| capture.clears_queue_refunds_prior_owner | MECHANICS§10 | Capture clears queue / refunds prior owner per rules | Systems | Mechanics Capture | Atomic | ok→atomic | i-sim |
| repair.dozer_pays_allied_building | MECHANICS§15 | Dozer repairs allied/own buildings paying cost | Orders/Systems | Mechanics Repair | Atomic | ok→atomic | i-sim |
| repair.factory_own_vehicles_only | MECHANICS§6/15 | Factory repairs own vehicles only | Systems | Mechanics Repair | Atomic | ok→atomic | i-sim |
| vet.ranks_and_heroic_self_heal | MECHANICS§9 | Ranks from kill value; elite/heroic self-heal | Systems | Mechanics Veterancy | Atomic | ok→atomic | i-sim |
| intel.enemy_queue_redacted | SIGHT / MECHANICS§11 fog | Enemy production queue redacted from viewer snapshot | Intel snapshots | Mechanics Intel | Atomic + client leak test | ok→atomic + client | i-sim, i-client |
| intel.radar_not_los | SIGHT | Radar ≠ LOS; brownout/radar rules | Intel/HUD minimap | Mechanics Intel | Atomic + HUD assertion | ok→atomic | i-sim, i-client |
| ai.no_hidden_army_cheat | MATCH_AI, SIGHT | AI uses hidden-info equality; no army cheat | Ai.cs | Mechanics Ai | Atomic | ok→atomic | i-sim |
| ai.jobs_base_gather_compose_attack_defend | MATCH_AI jobs, SLICE | AI runs base/gather/compose/attack/defend | Ai + FullMatch | FullMatch + audit jobs | Keep full match; optional job atomics | ok | i-sim |
| client.input_build_ghost_point | HUD `win.place`, orders Build | Real input path submits CanPlace-approved ghost point | MatchClient `_Input` → CommandIntent → Submit | ClientInputProof **bypasses Godot** | Godot/injected InputEvent through production `_Input` | **missing** GUI layer | i-client |
| client.guard_keeps_unit_target | MECHANICS Guard | Guard order keeps unit target via real command path | Client → Submit | helper / manual notes | Injected input scenario | **missing** | i-client |
| client.sell_mode_then_click | HUD `win.sell`, `ab.sell` | Sell mode then click own building | Client HUD | manual subset historically | Injected input scenario | **missing** | i-client |
| client.no_fog_world_leak | SIGHT | World/HUD picks do not reveal fog-hidden info | SelectionIntel / snapshots | helper SelectionIntel | Explicit leak tests on client path | **missing** | i-client |
| presentation.locomotion_root_motion | locomotion-requirements | Infantry/vehicles show root motion + facing while moving; structures static | Battlefield lerp only | no anim proof | Presentation + optional render frames; user visual accept | **missing** | i-client, i-assets, Q3 |
| presentation.chinook_rotor_spin | locomotion-requirements, manifest `rotor_*` | Chinook rotors spin while alive/operating | Battlefield ignores rotors | assemblies exist; no spin | Client spin ± allowlisted clip; asset verify | **missing** | i-client, i-assets |
| verify.entry_fail_closed | AGENTS verification | `tools/verify.sh` lists/selects scenarios; required missing ⇒ fail | proof.sh only | baseline proof groups | New entry point + structured JSON | **missing** | i-verify |
| verify.sealed_replay | determinism | Sealed input replay reproduces StateHash; field-level first divergence when claimed | FullMatch twin live controllers | hash AFF97B33… twin controllers | Sealed order-log replay package | **missing** | i-verify, i-sim, Q6 |
| verify.review_case_generalized | workflow | review_case parameterized; APPROVE/BLOCK parsed; missing proof ≠ pass | `tools/review_case.py` hardcoded slice-1 | process exit ≠ verdict | Generalized tool | **bug** (tool) | i-verify, Q10 |
| assets.no_skeletal_walk | locomotion-requirements, CONSTRAINTS glTF | No armature skeletal walk cycles | verify_assets forbids all anims | PASS 15 assets no anims | Allowlist policy; still ban skeletal walks | policy change | c-freeze, i-assets |

## Coverage notes

- Implemented SLICE systems (MECHANICS 1–11, 14–15 + delivery/damage/terrain/HUD/sight/AI slice) appear as rows above. Promotion/superweapons (12–13) and future factions are out of scope.
- Passing `tools/proof.sh` does **not** establish client input, render, or locomotion layers.
- Audit stuck/oscillate counts are **audit findings** until criteria are calibrated and explicitly gated.
- Visual/play acceptance remains **user** at close; engineering supplies package + bounded frame evidence where claimed.
