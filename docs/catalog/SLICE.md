# Slice-1

The first match, as a **filter** on this catalog. Not a second game. Not a 1v1-only engine.

Hardwired setup: you `aegis.vanilla` vs `ai.medium` `aegis.vanilla` on `maps.two_flats`. Superweapons off. Crates off. Two slots filled, rest closed.

Proof: a match you can finish. Win by destroying enemy buildings.

---

## Match

- [MATCH_AI.md](MATCH_AI.md): `mode.skirmish`, `set.fog` on, `set.superweapons` off, `set.crates` off, `set.start_units` = command + dozer.
- [TEAMS.md](TEAMS.md): two teams, no allies. Team field still exists.
- [SIGHT.md](SIGHT.md): shroud, fog, LOS, Aegis radar on command. No stealth. No height. No pulses.
- Pause: local vs AI. [HUD.md](HUD.md).

## Sim systems

Implement [MECHANICS.md](MECHANICS.md) **1–11 and 14–15** (match, orders, construction, economy, power, production, combat, garrison, veterancy, capture, stealth/intel *as fog only*, transport, repair). Skip 12–13 (promotion, superweapons) and 16–18 extras (faction toys, full loadouts).

Delivery: [DELIVERY.md](DELIVERY.md) slice. Damage: [DAMAGE.md](DAMAGE.md) slice.

**Incomplete building is lost.** [INVARIANTS.md](INVARIANTS.md).

## Roles

`build.dozer`, `eco.dropoff`, `eco.chinook`, `power.fusion`, `prod.command`, `prod.barracks`, `prod.factory`, `inf.rifle`, `inf.rocket`, `armor.basic`, `veh.scout_gun`, `def.patriot`, `map.dock`, `map.garrison`, `up.capture`.

Tree: [TECH_TREE.md](TECH_TREE.md) slice-1.

Chinook is `arm.air`. Patriot and rocket infantry must be able to kill it. Tanks must not.

## Garrison in this slice

`map.garrison` is on the map. No flashbang, flame, microwave, or combat-drop yet. Occupants spill when the **building is destroyed**. Do not invent a clear tool. Small-arms vs garrison stay **poor**.

## Abilities

`ab.build`, `ab.repair`, `ab.gather`, `ab.capture`, `ab.sell`.

## Statuses

`st.capturing`, `st.garrisoned`, `st.low_power`. No linger, hole, salvage.

## Terrain

`ter.ground`, `ter.unbuildable`, `ter.start`, `tag.garrisonable`, `tag.dock` on a **2-slot** map (`maps.two_flats`).

## HUD / voice / AI

- HUD: `hud.money`, `hud.power`, `hud.minimap`, `hud.selection`, `hud.command`, `cam.pan`, `cam.zoom`, `win.place`. No generals window.
- Voice: `vo.funds`, `vo.power`, `vo.building_done`, `vo.unit_ready`, `vo.under_attack`, `vo.victory`, `vo.defeat`. Beeps fine.
- AI: `job.base` `job.gather` `job.compose` `job.attack` `job.defend`. No powers.

## Still in the catalog (do not build yet)

Forge, Veil, specialists, promotion, superweapons, stealth, linger, 4/6/8 maps, LAN, Challenge.

The **data model** still has slots, teams, loadout ids, and prefix space for those rows. Do not delete them to make slice-1 compile.
