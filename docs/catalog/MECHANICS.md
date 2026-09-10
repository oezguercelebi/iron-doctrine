# Mechanics (grouped)

The sim. If a system is not in this file, it is not in the game.

Sides: **Aegis** (USA analog), **Forge** (China analog), **Veil** (GLA analog).

---

## 1. Match

- Real-time. Fog of war. One local player vs computer first. Later: up to 8 peers, no backend.
- Start: command structure + builder(s). Fogged map. One nearby supply dock.
- **Win:** destroy all enemy *buildings* (skirmish). Army wipe is recoverable.
- **Lose:** you have no buildings left, or you resign.
- Pause exists in local vs AI. Not a later multiplayer pause.

## 2. Orders

Every controllable thing answers a small verb set.

| Verb | Who | Notes |
| --- | --- | --- |
| Select / box / control group | Player | |
| Move | Units | |
| Attack | Units with a weapon | |
| Attack-move | Combat units | Engage on the way |
| Stop | Units | |
| Guard | Units | Follow / protect a target |
| Waypoint | Units | Queue path |
| Force-attack | Combat units | Ground or friendly-fire target |
| Build | Builder | Ghost + place. Anywhere legal. |
| Rally | Production buildings | |
| Queue / cancel | Production, research | |
| Sell | Own building | Refund a cut. Veil hole still applies unless noted. |
| Repair | Builder | Own / allied buildings. Clear mines. |
| Gather | Gatherer | Auto once a drop-off exists. |
| Enter / exit garrison | Infantry (most) | |
| Capture | Capture-capable infantry | Channel, not instant. |
| Ability | Unit or building | See [ABILITIES.md](ABILITIES.md) |
| Commander power | Player, on the map or as unlock | See group 12 |

No StarCraft-style attack/defend/hold stances. Artillery that must unpack (Forge nuke cannon analog) has **Deploy / Pack**.

## 3. Construction

- Buildings come from a **builder unit**, not a construction yard.
- **Aegis / Forge:** expensive dozer. One building per dozer. Repairs, clears mines.
- **Veil:** cheap worker. Builds *and* harvests. Many workers = many buildings at once.
- Place anywhere with space (and, for Aegis/Forge, eventual power to *run* it). Forward bases are legal.
- Ghost is blocked by terrain, other buildings, and steep cliffs unless a role says it can climb (Veil saboteur analog).
- Builders can be killed. Incomplete building is lost or left as a husk — pick one rule at slice-1 and keep it. Source: incomplete is lost.

## 4. Economy

### Primary: finite supply

- Map has **docks** (big, indestructible, default large pile) and **piles** (small).
- One gatherer **loads** at a dock at a time. Extra gatherers wait or go elsewhere.
- Gatherer returns to *your* drop-off. Distance is the tax.
- Dock empties. Then expand, steal, or go secondary.

| Side | Gatherer | Personality |
| --- | --- | --- |
| Aegis | Flying transport-gatherer | Fat load, ignores terrain, dies to AA |
| Forge | Ground truck | Comes free with drop-off, spam more |
| Veil | Worker | Same body as builder, slow, cheap |

### Secondary (forced by empty docks)

| Side | Passive print | Other |
| --- | --- | --- |
| All | Captured oil derrick | Oil refinery cheapens vehicles |
| Aegis | Timed air-drop pad (stalls if low power) | |
| Forge | Sit-hackers / internet building | Steal from enemy drop-off (hero / cash-hack power) |
| Veil | Black-market trickle | Salvage crates, cash-bounty power, steal-and-sell |

Supply Lines analog (Aegis ZH): a tech upgrade that fattens gather + drop-pad + oil.

## 5. Power

- **Aegis / Forge:** buildings draw from a pool. Low power: radar off, defenses off, some clocks stall.
- Aegis plant: clean. Per-building rod upgrade doubles (vanilla) or more (superweapon commander).
- Forge plant: bigger, **explodes** if killed or overcharged too long. Overcharge is a toggle for extra power at risk.
- **Veil: no power.** Identity. Never add a Veil plant.

## 6. Production

Shared skeleton, different chassis:

| Slot | Aegis | Forge | Veil |
| --- | --- | --- | --- |
| Command | Command | Command | Command |
| Power | Fusion | Reactor | — |
| Drop-off | Supply | Supply | Stash |
| Infantry | Barracks | Barracks | Barracks |
| Vehicles | Factory | Factory | Arms dealer |
| Air | Airfield | Airfield | — (no air force) |
| Tech | Strategy | Propaganda | Palace |
| Unique eco | Drop pad | Internet | Black market |
| Superweapon | Beam | Nuke silo | Missile storm |

- Queues + rally.
- Factory repairs vehicles that return to it.
- Airfield has a **pad cap**. Planes without a pad (or fuel analog for Forge jets) fall.
- One **hero** at a time per player.
- Soft army cap is cash + build time. Engine needs a hard unit cap; number later.

## 7. Combat

Readable RPS. A blob of one role dies to its answer.

**Axes:** infantry · light vehicle · tank · artillery · air · structure · garrison.

**Damage kinds** (sim tags, not UI): small-arms, cannon, rocket, explosive, flame, toxin, radiation, laser, EMP, sniper, crush, microwave.

**Rules that must exist:**

- Tanks **crush** infantry they drive over (heroes and some roles immune).
- Rockets beat tanks and air; lose to small-arms and anti-infantry vehicles.
- Artillery outranges defenses, is fragile, often cannot fire on the move (or unpacks).
- Air ignores ground pathing. Dies to dedicated AA. Aegis is the air side. Forge is thin air. Veil has **zero** air and the best organic AA.
- Splash and lingering fields (flame, toxin, radiation) are first-class, not VFX.
- Missiles can be **shot down** (Aegis paladin laser, Veil stinger / RPG).
- One gatherer loading at a dock is a combat target.

## 8. Garrison

- Civilian buildings hold a squad (source: ~8 vanilla, ~10 ZH). Building takes hits; occupants spill out hurt when the building is too damaged.
- You do **not** clear a garrison by plinking the walls forever. Clear with flame, toxin, microwave, flashbang, or combat-drop.
- Purpose-built: Aegis firebase, Forge bunker, Veil palace.
- Sniper roles can sit in a civilian building **without flipping its team color**.
- Some infantry are useless inside (hackers, workers, suicide, hijacker, saboteur, hero gadgets). They hide, they do not fight.

## 9. Veterancy

Three ranks: Veteran → Elite → Heroic.

- Earned from value of kills (and some capture / hack).
- Each rank: more damage and rate of fire; health bumps.
- Elite and Heroic **self-heal**.
- Some powers and some commanders **train already veteran**.
- Aegis tech: units rank faster (Advanced Training analog).

## 10. Capture

- Basic rifle infantry research **Capture**. Then channel on a building.
- Not instant. Interruptible. Building flips to you.
- Neutral tech: oil, refinery, hospital, ZH pads (repair, artillery, reinforcement).
- Enemy production buildings can be stolen. Veil can steal a dozer via hijack and build the *other* side's base.
- Veil ZH: booby-trap a building so a capture attempt kills the capper.

## 11. Stealth, detection, intel

- Fog of war + shroud. Radar fills the minimap for detected areas.
- Aegis: radar on command from start. Spy satellite on a short cooldown.
- Forge: radar is a **command upgrade**. Starts blind on the minimap.
- Veil: radar is a **van**. Kill the van, lose the map.
- Stealth: snipers, heroes, Veil camo, disguised bomb truck, fake buildings, stealth air, Kassad-style almost-everything.
- Detection: Aegis drones / pathfinders / sentry; Forge troop crawler / listening outpost; Veil radar van.
- A match vs Veil without detection is a different game. Detection is a role, not a nice-to-have.

## 12. Commander promotion

Combat XP fills **the player**, not only the unit.

Ranks: 1-star, 3-star, 5-star. Skill points. More powers than points.

A power is one of:

- **Unlock** a unit
- **Passive** (train veteran, cash bounty)
- **Click on the map** with a cooldown (strike, drop, reveal, ambush, repair)

Shared click: **Emergency Repair** (area vehicle heal), 3 ranks.

Campaign trees in the source differ from skirmish. We use **one skirmish tree** per loadout.

## 13. Superweapons

A building. Visible. Long build, long charge, announced.

| Side | Character |
| --- | --- |
| Aegis | Steerable beam. Precision. Can delete Veil holes. |
| Forge | Huge nuke + radiation. |
| Veil | Volley of missiles, explosive and/or toxin. |

Killing the building resets the clock. Racing two clocks is a late-game mode.

Aegis tech building also has **battle plans** (one global stance at a time: damage / armor / vision). Not a superweapon.

## 14. Transport

- Capacity slots. Infantry 1, vehicles more, dozers a lot.
- Aegis flying gatherer is also a transport. **Combat-drop** rifle infantry into a garrisoned building to take it.
- Forge troop crawler: transport + stealth detect + infantry heal. Comes loaded.
- Veil technical / battle bus / combat cycle: cheap, nasty, infantry-on-a-bike inherits the rider's weapon.

## 15. Repair, heal, linger

- Builders repair buildings.
- Factory repairs vehicles.
- Aegis ambulance: heal infantry, **clean toxin/radiation**. ZH: also repair vehicles.
- Forge speaker tower: area heal. Overlord can mount a speaker.
- Veil junk-repair upgrade: vehicles heal in the field.
- Linger fields: napalm firestorm, toxin pool, radiation, microwave bubble. Some sides can clean them. Veil wants you to stand in them.

## 16. Faction rules (not optional flavor)

### Aegis — drones and plans

- Ground vehicles hang one drone: scout (vision), battle (repair / gun), or hellfire (anti-vehicle).
- Battle plans at tech: bombardment / hold the line / search and destroy.

### Forge — horde, fire, mines

- **Horde:** 5+ rifle, rocket, or basic tanks in a clump shoot faster. Nationalism analog fattens it.
- Napalm stacks into a **firestorm**.
- Buildings can be ringed with **land mines** (neutron mines later: kill infantry and crews).
- Reactors are bombs if you snipe them.

### Veil — hole, salvage, tunnels, no grid

- Destroyed buildings leave a **hole**. If you do not crush it, the building rebuilds. Beam superweapon deletes holes.
- **Salvage crates** from wrecks: Technical / Quad / Marauder pick them up and grow a gun.
- **Tunnel network:** enter any node, exit any other. Sneak-attack power drops a new entrance.
- Fake buildings (ZH): cheap decoys, can be upgraded into real.
- Demo traps: stealthed mines.
- No air. No power.

## 17. Map language

A map is not art. It is slots, docks, garrison, and terrain. **Max 8 players.** 1v1 is one size, not the product.

See [MAP_GRAMMAR.md](MAP_GRAMMAR.md) and [MAPS.md](MAPS.md).

- Start spots = map max (2 / 4 / 6 / 8)
- Close dock per start + contest docks that scale
- Garrison belts on docks and chokes
- Neutral tech in contest space
- Cliffs / water that block ground and not air
- Teams and FFA are match settings on those slots

## 18. Loadouts

Vanilla three sides, plus a 3×3 specialist grid (Zero Hour). 12 playable. Boss mix is AI-only later.

A specialist is **buffs, a unique toy, and a hole**. See ROLES.md loadout grid.
