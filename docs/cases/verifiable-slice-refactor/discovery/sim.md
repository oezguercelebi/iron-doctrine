# Discovery: simulation / contracts / determinism

Explore `01a0a1dc-dff1-70a2-8500-610aa011bf7a` · grok-4.5 · effort high · read-only.
Tree assumed `c5aefdd`. Lead additionally ran proofs on this tree (see `proof/baseline-commands.md`).

Lead confirmation (executed, not just source-read):
- `bash tools/proof.sh` exit 0; full match defeat tick 2033; hash `AFF97B33D0FECB7208E09F6869EDA724E97384863B4137450AD977D751317842`
- `bash tools/audit.sh` exit 0; `stuck=12 oscillate=42` on the same finished match

## Confirmed catalog drift

`ter.unbuildable`: catalog `TERRAIN.md` ground yes / build no. Code `GameConfig.cs:138` and `Pathing.TerrainFits` treat it as ground-impassable. Closed-case contract agreed with code. **Catalog wins** — default is repair pathing, not rewrite the catalog.

## Historical leads

| Lead | Verdict |
|---|---|
| Full match pass + stuck/oscillate diagnostics | **Still true** (lead re-ran audit) |
| `ter.unbuildable` blocking ground | **Still true** (source) |
| Instant combat tracers from Activity+cooldown | **Still true** (`Battlefield.cs`); missiles have projectile ids |
| Acceptance.md vs closed revision | **Still true** (docs diverge) |

## Determinism

`FullMatchProof` is twin live controllers, not a sealed order-log replay. `StateHash` covers C,S,Tick,random,nextId,nextShotId,topology,paused,phase,winners,pending,shots,events,Bodies,Players,brains. First-divergent-tick only; no field-level diff. `NextRandom` has no gameplay callers.

## Combat

`inf.rocket` missile kills `eco.chinook`; `armor.basic` rejected (`Orders.cs:61`, `MechanicsProof.Combat`). Instant Hit same tick; missile world Shot. No dedicated duplicate-impact negative control.

## Suggested seams for C

`src/Contracts/MatchContract.cs`, `GameConfig.cs`, `CatalogIds.cs`, `data/slice1.placeholders.json`, `tests/SeamConformance.cs`, proof/scenario runner, `ter.unbuildable` policy.

Full explore body (verbatim summary retained in session). Atomic scenario names proposed by explore (verification ids, not catalog prefixes):

match.boot_command_dozer, match.pause_freezes_clock, match.resign_is_loss, match.army_wipe_recoverable, match.elim_by_buildings_only, build.prereq_blocks_factory, build.incomplete_lost_on_builder_death, build.ghost_matches_arrival_occupancy, build.facing_sets_default_rally, eco.dock_exclusive_loader, eco.return_own_dropoff_only, eco.free_chinook_once_on_build_not_capture, eco.sell_refund_cut, power.brownout_stops_radar_and_patriot, prod.queue_cancel_refund, prod.rally_followed, combat.tank_cannot_hit_chinook, combat.rocket_missile_kills_chinook, combat.missile_exists_before_impact, combat.no_duplicate_missile_impact, combat.rifle_beats_rocket_even_fight, combat.rockets_counter_tank, combat.crush_ff_on_allied_infantry, combat.force_attack_friendly, garrison.enter_flips_neutral_owner, garrison.small_arms_poor, garrison.destroy_spills_hurt, transport.scout_passengers_fire, transport.chinook_passengers_silent, transport.exit_local_reach, capture.channel_interruptible, capture.clears_queue_refunds_prior_owner, repair.dozer_pays_allied_building, repair.factory_own_vehicles_only, vet.ranks_and_heroic_self_heal, intel.enemy_queue_redacted, intel.radar_not_los, path.air_ignores_ground_clutter, path.ground_traverses_or_blocks_unbuildable, path.distinguish_wait_vs_stuck, ai.no_hidden_army_cheat, ai.jobs_base_gather_compose_attack_defend, presentation.instant_tracer_tied_to_resolved_hit, client.guard_keeps_unit_target, client.sell_mode_then_click.
