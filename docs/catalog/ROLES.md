# Roles

Every thing that exists in the match. Grouped with [MECHANICS.md](MECHANICS.md). No HP, cost, or product names.

**side:** `Aegis` `Forge` `Veil` `All` `Map`, or a commander id (`aegis.air`, `aegis.laser`, `aegis.super`, `forge.tank`, `forge.infantry`, `forge.nuke`, `veil.toxin`, `veil.demo`, `veil.stealth`).

Armor, damage, delivery: **Combat join** below. Doctrines: [FACTIONS.md](FACTIONS.md).

Vanilla roles apply to that side's specialists unless the loadout grid says **drop** or **replace**.

---

## Construction

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `build.dozer` | Heavy builder | Aegis, Forge | Construction Dozer | — | Anything. Especially air, rockets | One building at a time. Repair. Clear mines. Expensive. Hijackable. |
| `build.worker` | Worker-builder-gatherer | Veil | Worker | — | Small-arms, splash, crush | Builds *and* harvests. Many at once. Shoes upgrade speeds them. |

---

## Economy — buildings

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `eco.dropoff` | Supply drop-off | Aegis, Forge | Supply Center | — | Raid, steal-cash, air | Spawns the first gatherer. Trains more gatherers. |
| `eco.stash` | Supply stash | Veil | Supply Stash | — | Raid | Trains workers. Drop-off. |
| `eco.drop_pad` | Timed crate pad | Aegis | Supply Drop Zone | — | Air, low power (clock stalls) | Secondary income. Needs power. |
| `eco.internet` | Hack farm building | Forge | Internet Center | — | Raid, toxin, splash | ZH. Houses hackers as a structure. Satellite-hack upgrades. |
| `eco.black_market` | Trickle + upgrade shop | Veil | Black Market | — | Raid (high value) | Money over time *and* late upgrades. |

## Economy — units

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `eco.chinook` | Air gatherer + transport | Aegis | Chinook | — | AA, fighters | Fat load. Combat-drop rifle into garrisons. Empty to gather. |
| `eco.truck` | Ground gatherer | Forge | Supply truck | — | Anything | Free with drop-off. Spam. |
| `eco.hacker` | Sit-printer / disable | Forge | Hacker | Buildings (disable) | Any combat, splash | Fragile. Prints more as they vet. Internet building is the ZH version. |

---

## Power

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `power.fusion` | Clean plant | Aegis | Cold Fusion Reactor | — | Snipe (base goes dark) | Rod upgrade on the building. Super commander: stronger rods. |
| `power.reactor` | Dirty plant | Forge | Nuclear Reactor | Linger (if it pops) | Snipe — explosion hurts *you* | Overcharge toggle. Extra power, then boom. |
| `power.none` | No grid | Veil | — | — | — | Not a building. Faction rule. |

---

## Production & command

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `prod.command` | Command | All | Command Center | — | Superweapon, mass | Trains builder. Powers fire from here. Aegis: radar + satellite. Forge: radar upgrade. Veil: hole. |
| `prod.barracks` | Infantry | All | Barracks | — | Raid | Capture research lives here. |
| `prod.factory` | Vehicles | Aegis, Forge | War Factory | — | Raid, artillery | Repairs vehicles. |
| `prod.arms` | Vehicles | Veil | Arms Dealer | — | Raid | Same slot as factory. |
| `prod.airfield` | Aircraft pads | Aegis, Forge | Airfield | — | Raid, sneak | Pad cap. No Veil airfield. |
| `prod.tech.aegis` | Tech + battle plans | Aegis | Strategy Center | — | Raid | Unlocks late units. One global plan. |
| `prod.tech.forge` | Tech + propaganda | Forge | Propaganda Center | — | Raid | Unlocks late units. Nationalism. |
| `prod.tech.veil` | Tech + garrison | Veil | Palace | Infantry (garrisoned) | Flame, toxin, artillery | Garrison 5. Late unlocks. |
| `prod.detention` | Intel building | Aegis (vanilla Generals) | Detention Camp | — | Raid | Brief map reveal power. Optional for us; ZH mostly moved intel to powers. |

---

## Defense buildings

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `def.patriot` | Missile turret | Aegis | Patriot | Air, vehicles | Infantry, artillery | Link with neighbors. Super commander: EMP missiles. Laser commander: laser turret **replaces** this. |
| `def.firebase` | Howitzer nest | Aegis | Fire Base | Ground at range; garrison 4 | Air, artillery, rush | ZH. Cannon + infantry holes. |
| `def.gatling` | Bullet turret | Forge | Gatling Cannon | Infantry, air | Tanks, artillery | Powered. Chain-gun upgrade. |
| `def.bunker` | Infantry box | Forge | Bunker / Fortified | Whatever is inside | Flame, toxin, artillery | Infantry commander: bigger bunker. |
| `def.speaker` | Heal tower | Forge | Speaker Tower | — | Anything | Area heal. Needs tech. |
| `def.stinger` | Crewed AA/AT nest | Veil | Stinger Site | Air, tanks | Sniper (kills crew), infantry clear | Crewed. Rebuilds via hole. |
| `def.tunnel` | Defense + subway | Veil | Tunnel Network | Infantry (gun) | Tanks, artillery | Global travel. Toxin commander: toxin gun. |
| `def.trap` | Stealthed mine | Veil | Demo Trap | Vehicles, clumps | Detector + gun | Demo commander: stronger. Toxin: toxic. |
| `def.fake` | Decoy building | Veil | Fake building | Intel (wastes shots) | Detector, ignore | ZH. Can upgrade into the real building. |

---

## Infantry

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `inf.rifle` | Rifle | All | Ranger / Red Guard / Rebel | Infantry, light | Tanks, splash, sniper, garrison-clear | Capture (after research). Aegis: flashbang + combat-drop. Forge: pair-built, horde. Veil: camo upgrade. |
| `inf.rocket` | Rocket | All | Missile Defender / Tank Hunter / RPG | Tanks, air, buildings | Small-arms, anti-infantry vehicles | Aegis: laser-lock. Forge: TNT on vehicles, horde. Veil: intercepts some missiles. |
| `inf.sniper` | Sniper | Aegis | Pathfinder | Infantry | Vehicles, detection + gun | Stealthed standing. Unlock (vanilla). Garrison without flipping color. |
| `inf.suicide` | Contact bomb | Veil | Terrorist | Clumped vehicles, buildings | Small-arms, gatling, flashbang | Can enter civilian cars. Demo commander: stronger. Toxin: toxin bomber. |
| `inf.mob` | Swarm blob | Veil | Angry Mob | Everything if upgraded and many | Air, splash, flame | Cannot use tunnels. Arm-the-mob upgrade is mandatory if you commit. |
| `inf.hijack` | Steal vehicle | Veil | Hijacker | Isolated vehicles (not high-vet) | Detection, infantry screen | Unlock. Stealthed still. Steal a dozer = build their side. Stealth commander: from start. Dropped by toxin/demo. |
| `inf.saboteur` | Infiltrate / reset | Veil | Saboteur | Buildings (power down, reset powers if command) | Detection, patrols | ZH. Climbs cliffs. Toxin drops this. |
| `inf.minigun` | Heavy rifle | forge.infantry | Mini-Gunner | Infantry, light, some air | Tanks, artillery | Replaces rifle as the mass body. |
| `inf.toxin_rifle` | Toxin rifle | veil.toxin | Toxin Rebel | Infantry, linger | Same as rifle; less vs armor | Replaces rifle. Immune to toxin. |
| `inf.pilot` | Ejected crew | Aegis | Pilot | — | Small-arms | Not trained. Spawns from a dead vehicle/plane. Keeps that unit's vet. Enter a friendly vehicle to pass the rank on. |

---

## Heroes (one at a time)

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `hero.demo_stealth` | Commando | Aegis | Colonel Burton | Infantry, buildings (charges) | Detection, vehicles | Knife, charges, stealth. |
| `hero.capture` | Infiltrator | Forge | Black Lotus | Buildings, cash, disabled vehicles | Detection, patrols | Steal building, disable vehicle, steal drop-off cash. No gun. |
| `hero.sniper` | Ace sniper | Veil | Jarmen Kell | Infantry; vehicle *drivers* | Detection, air, flashbang | Stealthed standing. Empty tank can be stolen. Garrison without flipping color. |

---

## Light vehicles & support

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `veh.scout_gun` | Fast gun + transport | Aegis | Humvee | Infantry | Tanks, rockets | Holds 5 who shoot out. TOW upgrade. Drones. |
| `veh.ambulance` | Cleanse / heal | Aegis | Ambulance | Linger fields | Anything | Heals infantry. Clears toxin/rad. ZH: repairs vehicles. |
| `veh.technical` | Scrap truck | Veil | Technical | Infantry | Tanks, rockets | Salvage crates upgrade the gun. Transport. |
| `veh.buggy` | Hit-and-run rockets | Veil | Rocket Buggy | Light, structures, kiting tanks | Anything that catches it | Needs tech. Reload between volleys. `up.buggy_ammo`. |
| `veh.quad` | Organic AA | Veil | Quad Cannon | Air, infantry | Tanks | Salvage upgrades RoF. The reason Aegis air dies. |
| `veh.toxin_spray` | Spray clear | Veil | Toxin Tractor | Infantry, garrison | Tanks, air | Clears buildings. Toxin commander: default identity. |
| `veh.radar` | Minimap + detect | Veil | Radar Van | Stealth (detect) | Anything | No van, no radar. Scan ability. |
| `veh.cycle` | Infantry on a bike | Veil | Combat Cycle | Depends on rider | Small-arms, tanks | ZH. Inherits rider weapon (rocket, suicide, sniper…). |
| `veh.bus` | Armored bus | Veil | Battle Bus | Infantry while garrisoned | Rockets, tanks | ZH. Wreck can still fight as a bunker until crushed. |
| `veh.listening` | Detect truck | Forge | Listening Outpost | Stealth (detect) | Anything | ZH. Stealthed. Comes with rockets inside. Fragile. |
| `veh.ecm` | Jammer | Forge | ECM Tank | Missiles, vehicles (disable) | Tanks if ignored | ZH. Missiles miss. Shuts vehicles down. |
| `veh.microwave` | Bubble + shut-down | Aegis | Microwave Tank | Infantry, garrison, buildings (offline) | Tanks, rockets | ZH. Clears garrison. Turns buildings off. |
| `veh.avenger` | Laser AA + buff | Aegis | Avenger | Missiles, air; paints ground | Tanks | ZH. Speeds friendly ground fire. Laser commander: cheaper. |
| `veh.sentry` | Stealthed detector | Aegis | Sentry Drone | Stealth (detect); infantry if gunned | Detection + gun, EMP | ZH. Gun is an upgrade. |
| `veh.crawler` | Detect transport | Forge | Troop Crawler | Stealth (detect) | Rockets, tanks | Comes loaded with rifle. Heals infantry inside. Infantry commander: assault variant. |

Aegis ground combat vehicles hang **one** drone. See [ABILITIES.md](ABILITIES.md) add-ons (`addon.drone.*`).

Leftover entities (`ent.hole`, `ent.salvage`, linger fields) are in [STATUSES.md](STATUSES.md), not here.

---

## Armor

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `armor.basic` | Basic tank | All | Crusader / Battlemaster / Scorpion | Vehicles, buildings, crush | Rockets, air, artillery | Forge: horde. Veil: weaker until rocket + toxin shells. Laser commander: **replace** with laser tank. Super commander: **drop**. |
| `armor.elite` | Missile-defense tank | Aegis | Paladin | Tanks; shoots down missiles | Rockets, air | Unlock. Laser commander: **drop**. |
| `armor.laser` | Laser tank | aegis.laser | Laser Tank | Vehicles, air (beam) | Mass rockets, EMP | Replaces basic. No Paladin, no Tomahawk. |
| `armor.marauder` | Scrap tank | Veil | Marauder | Vehicles | Air, rockets (still) | Unlock. Salvage grows the gun. Stealth commander: **drop**. |
| `armor.flame` | Flame tank | Forge | Dragon Tank | Infantry, garrison, firestorm | Tanks, rockets | Black napalm. Firewall ability. |
| `armor.gatling` | Bullet tank | Forge | Gatling Tank | Infantry, air, early harass | Tanks | Best Forge opener. Chain guns. |
| `armor.mammoth` | Superheavy | Forge | Overlord | Everything on ground if supported | Air, suicide, hijack, bomb truck | Slow. Add-on: bunker / gatling / speaker. Tank commander: Emperor variant. |
| `armor.emperor` | Command mammoth | forge.tank | Emperor Overlord | Same + aura | Same | Replaces mammoth. |

---

## Artillery

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `arty.missile` | Cruise missile | Aegis | Tomahawk | Buildings, garrison, defenses | AA that eats missiles, rush | Fragile. Laser commander: **drop**. |
| `arty.napalm` | Napalm barrage | Forge | Inferno Cannon | Infantry, defenses, firestorm | Air, rush | Tank commander: **drop**. |
| `arty.tacnuke` | Unpack nuke gun | Forge | Nuke Cannon | Clumps, buildings, linger rad | Air, rush, disable | Unlock. Cannot fire packed. Neutron shells ZH. Tank commander: **drop**. Nuke commander: identity. |
| `arty.scud` | Toggle missile | Veil | Scud Launcher | Buildings (HE) or blobs (toxin) | Air, rush | Unlock. Stealth commander: **drop**. Toxin: toxin only. |

---

## Air

Veil has none. AA is their air force.

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `air.fighter` | Strike fighter | Aegis | Raptor | Vehicles, buildings | AA, fighters | Reload at pad. Laser missiles. Air commander: King variant. |
| `air.king` | Fat fighter | aegis.air | King Raptor | Same + stronger | AA | Replaces fighter. |
| `air.stealth` | Stealthed strike | Aegis | Stealth Fighter | Defenses, buildings | Detector AA, pad loss | Unlock. Visible on attack. |
| `air.bomber` | Fast structure bomb | Aegis | Aurora | Buildings | AA on the slow return | After drop, sluggish. Super commander: Alpha (fuel-air bomb). |
| `air.alpha` | Fuel-air bomber | aegis.super | Aurora Alpha | Buildings, clumps | Same return-death | Replaces bomber. |
| `air.heli` | Loiter gunship | Aegis | Comanche | Infantry + vehicles | AA | Reloads rockets in the field. Air commander: stealth heli. |
| `air.combat_chinook` | Armed transport | aegis.air | Combat Chinook | Light ground | AA | Replaces/upgrades chinook as a gunship-transport. |
| `air.mig` | Napalm jet | Forge | MiG | Ground, firestorm | AA | Thin air force. Nuke commander: nuke warhead. |
| `air.helix` | Flying mammoth | Forge | Helix | Ground, transport, support | AA | ZH. Add-ons like Overlord (bunker / gatling / speaker). |

---

## Suicide / deception (Veil)

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `deceive.bomb_truck` | Disguised bomb | Veil | Bomb Truck | Clumps, bases | Detector, AA (if spotted) | Disguise as any vehicle. HE and/or bio payload. Toxin: bio only. Demo: HE identity. |

---

## Superweapons

| id | role | side | analog | beats | beaten by | notes |
| --- | --- | --- | --- | --- | --- | --- |
| `sw.beam` | Steerable beam | Aegis | Particle Cannon | Buildings, holes, precision | Snipe the building, race | Super commander: cheaper / spam. Only super that cleanly deletes Veil holes. |
| `sw.nuke` | City-killer | Forge | Nuclear Missile Silo | Base + linger rad | Snipe, race | Unlocks tank nuke-upgrades. |
| `sw.storm` | Missile volley | Veil | Scud Storm | Base + toxin | Snipe, race | Anthrax upgrades fatten it. |

---

## Neutral map objects

| id | role | analog | notes |
| --- | --- | --- | --- |
| `map.dock` | Finite supply | Supply dock | Indestructible. One loader. Default large pile. |
| `map.pile` | Small supply | Supply pile | |
| `map.oil` | Passive cash | Oil derrick | Capture. |
| `map.refinery` | Cheaper vehicles | Oil refinery | Capture. |
| `map.hospital` | Infantry auto-heal | Hospital | Capture. |
| `map.garrison` | Civilian box | Houses, etc. | Role 8. Snipers do not flip color. |
| `map.car` | Stealable car | Civilian vehicle | Veil suicide enters → car bomb. |
| `map.crate` | Cash crate | UN crate | Pick up. |
| `map.repair_bay` | Global vehicle regen | Tech Repair Bay (ZH) | Capture. |
| `map.repair_pad` | Park-to-repair | Repair Pad (ZH) | Capture. |
| `map.arty_plat` | Captured gun | Artillery Platform (ZH) | Auto-fires. Min range. |
| `map.reinforce` | Periodic free vehicle | Reinforcement Pad (ZH) | Vehicle depends on loadout. |

---

## Combat join

Primary hull, outgoing damage, delivery. `—` = none (does not shoot, or cannot be shot). Occupied garrison is a *state*: hits use `arm.garrison` until the box breaks. Prefixes: [DAMAGE.md](DAMAGE.md), [DELIVERY.md](DELIVERY.md).

`power.none` is not a placeable. Skip it.

| id | arm | dmg | del |
| --- | --- | --- | --- |
| `build.dozer` | `arm.light` | — | — |
| `build.worker` | `arm.infantry` | — | — |
| `eco.dropoff` | `arm.structure` | — | — |
| `eco.stash` | `arm.structure` | — | — |
| `eco.drop_pad` | `arm.structure` | — | — |
| `eco.internet` | `arm.structure` | — | — |
| `eco.black_market` | `arm.structure` | — | — |
| `eco.chinook` | `arm.air` | — | — |
| `eco.truck` | `arm.light` | — | — |
| `eco.hacker` | `arm.infantry` | — | — |
| `power.fusion` | `arm.structure` | — | — |
| `power.reactor` | `arm.structure` | — | — |
| `prod.command` | `arm.structure` | — | — |
| `prod.barracks` | `arm.structure` | — | — |
| `prod.factory` | `arm.structure` | — | — |
| `prod.arms` | `arm.structure` | — | — |
| `prod.airfield` | `arm.structure` | — | — |
| `prod.tech.aegis` | `arm.structure` | — | — |
| `prod.tech.forge` | `arm.structure` | — | — |
| `prod.tech.veil` | `arm.structure` | — | — |
| `prod.detention` | `arm.structure` | — | — |
| `def.patriot` | `arm.structure` | `dmg.rocket` | `del.missile` |
| `def.firebase` | `arm.structure` | `dmg.cannon` | `del.arc` |
| `def.gatling` | `arm.structure` | `dmg.small` | `del.instant` |
| `def.bunker` | `arm.structure` | — | — |
| `def.speaker` | `arm.structure` | — | — |
| `def.stinger` | `arm.structure` | `dmg.rocket` | `del.missile` |
| `def.tunnel` | `arm.structure` | `dmg.small` | `del.instant` |
| `def.trap` | `arm.structure` | `dmg.explosive` | `del.melee` |
| `def.fake` | `arm.structure` | — | — |
| `inf.rifle` | `arm.infantry` | `dmg.small` | `del.instant` |
| `inf.rocket` | `arm.infantry` | `dmg.rocket` | `del.missile` |
| `inf.sniper` | `arm.infantry` | `dmg.sniper` | `del.instant` |
| `inf.suicide` | `arm.infantry` | `dmg.explosive` | `del.melee` |
| `inf.mob` | `arm.infantry` | `dmg.small` | `del.instant` |
| `inf.hijack` | `arm.infantry` | — | — |
| `inf.saboteur` | `arm.infantry` | — | — |
| `inf.minigun` | `arm.infantry` | `dmg.small` | `del.instant` |
| `inf.toxin_rifle` | `arm.infantry` | `dmg.toxin` | `del.instant` |
| `inf.pilot` | `arm.infantry` | — | — |
| `hero.demo_stealth` | `arm.infantry` | `dmg.small` | `del.instant` |
| `hero.capture` | `arm.infantry` | — | — |
| `hero.sniper` | `arm.infantry` | `dmg.sniper` | `del.instant` |
| `veh.scout_gun` | `arm.light` | `dmg.small` | `del.instant` |
| `veh.ambulance` | `arm.light` | — | — |
| `veh.technical` | `arm.light` | `dmg.small` | `del.instant` |
| `veh.buggy` | `arm.light` | `dmg.rocket` | `del.missile` |
| `veh.quad` | `arm.light` | `dmg.small` | `del.instant` |
| `veh.toxin_spray` | `arm.light` | `dmg.toxin` | `del.spray` |
| `veh.radar` | `arm.light` | — | — |
| `veh.cycle` | `arm.light` | — | — |
| `veh.bus` | `arm.light` | — | — |
| `veh.listening` | `arm.light` | `dmg.rocket` | `del.missile` |
| `veh.ecm` | `arm.tank` | — | — |
| `veh.microwave` | `arm.tank` | `dmg.microwave` | `del.spray` |
| `veh.avenger` | `arm.light` | `dmg.laser` | `del.beam` |
| `veh.sentry` | `arm.light` | — | — |
| `veh.crawler` | `arm.light` | — | — |
| `armor.basic` | `arm.tank` | `dmg.cannon` | `del.instant` |
| `armor.elite` | `arm.tank` | `dmg.cannon` | `del.instant` |
| `armor.laser` | `arm.tank` | `dmg.laser` | `del.beam` |
| `armor.marauder` | `arm.tank` | `dmg.cannon` | `del.instant` |
| `armor.flame` | `arm.tank` | `dmg.flame` | `del.spray` |
| `armor.gatling` | `arm.tank` | `dmg.small` | `del.instant` |
| `armor.mammoth` | `arm.tank` | `dmg.cannon` | `del.instant` |
| `armor.emperor` | `arm.tank` | `dmg.cannon` | `del.instant` |
| `arty.missile` | `arm.arty` | `dmg.rocket` | `del.missile` |
| `arty.napalm` | `arm.arty` | `dmg.flame` | `del.arc` |
| `arty.tacnuke` | `arm.arty` | `dmg.rad` | `del.arc` |
| `arty.scud` | `arm.arty` | `dmg.explosive` | `del.missile` |
| `air.fighter` | `arm.air` | `dmg.rocket` | `del.missile` |
| `air.king` | `arm.air` | `dmg.rocket` | `del.missile` |
| `air.stealth` | `arm.air` | `dmg.rocket` | `del.missile` |
| `air.bomber` | `arm.air` | `dmg.explosive` | `del.drop` |
| `air.alpha` | `arm.air` | `dmg.explosive` | `del.drop` |
| `air.heli` | `arm.air` | `dmg.small` | `del.instant` |
| `air.combat_chinook` | `arm.air` | `dmg.small` | `del.instant` |
| `air.mig` | `arm.air` | `dmg.flame` | `del.drop` |
| `air.helix` | `arm.air` | `dmg.small` | `del.instant` |
| `deceive.bomb_truck` | `arm.light` | `dmg.explosive` | `del.melee` |
| `sw.beam` | `arm.structure` | `dmg.laser` | `del.beam` |
| `sw.nuke` | `arm.structure` | `dmg.rad` | `del.arc` |
| `sw.storm` | `arm.structure` | `dmg.explosive` | `del.missile` |
| `map.dock` | — | — | — |
| `map.pile` | — | — | — |
| `map.oil` | `arm.structure` | — | — |
| `map.refinery` | `arm.structure` | — | — |
| `map.hospital` | `arm.structure` | — | — |
| `map.garrison` | `arm.garrison` | — | — |
| `map.car` | `arm.light` | — | — |
| `map.crate` | — | — | — |
| `map.repair_bay` | `arm.structure` | — | — |
| `map.repair_pad` | `arm.structure` | — | — |
| `map.arty_plat` | `arm.structure` | `dmg.cannon` | `del.arc` |
| `map.reinforce` | `arm.structure` | — | — |

Indestructible: `map.dock` (and usually `map.pile` as a resource, not a hull).

Second weapons (not a second id):

| id | extra |
| --- | --- |
| `veh.scout_gun` | `up.tow` adds `dmg.rocket` / `del.missile` |
| `armor.elite` | `ab.pdl` is `dmg.intercept`, not the main gun |
| `veh.avenger` | Also eats missiles (`dmg.intercept`) |
| `air.heli` | `up.pods` adds `dmg.rocket` / `del.missile` |
| `veh.sentry` | `up.sentry_gun` adds `dmg.small` / `del.instant` |
| `veh.cycle` | Inherits rider weapon |
| `arty.scud` | Toxin toggle → `dmg.toxin` |
| `deceive.bomb_truck` | Bio payload → `dmg.toxin` |
| `sw.storm` | Anthrax fattens toxin; still not interceptable |
| Occupied bunker / firebase / palace / bus | Occupants shoot; hull stays `arm.structure` until `arm.garrison` rules apply |

---

## Upgrades (global research)

Not units. They change counters. Grouped by side.

### All

| id | role | analog | does |
| --- | --- | --- | --- |
| `up.capture` | Capture unlock | Capture Building | Rifle can channel-capture. |

### Aegis

| id | role | analog | does |
| --- | --- | --- | --- |
| `up.flashbang` | Garrison clear | Flashbang Grenades | Rifle toggles grenades. Beats garrison and mobs. |
| `up.tow` | Humvee AT | TOW Missile | Scout gun beats tanks. |
| `up.composite` | Tank armor | Composite Armor | Basic / elite / avenger / microwave. |
| `up.drone_armor` | Drone HP | Drone Armor | |
| `up.fast_vet` | Double XP | Advanced Training | |
| `up.supply_lines` | Fatter eco | Supply Lines | Gather, drop pad, oil. |
| `up.chem_suits` | Anti-linger | Chemical Suits | Infantry vs toxin/rad. |
| `up.rods` | Double plant | Control Rods | Per fusion. Super: advanced rods. |
| `up.laser_msl` | Air damage | Laser Missiles | Fighter + stealth fighter. |
| `up.flares` | Anti-missile air | Countermeasures | Planes dodge. |
| `up.pods` | Heli volley | Rocket Pods | |
| `up.sentry_gun` | Detector shoots | Sentry gun | |
| `up.bunker_buster` | Stealth vs garrison | Bunker Busters | |

### Forge

| id | role | analog | does |
| --- | --- | --- | --- |
| `up.horde` | Horde exists | (innate) | 5+ rifle / rocket / basic tank. |
| `up.nationalism` | Fatter horde | Nationalism | |
| `up.chain` | Bullet damage | Chain Guns | Gatling tank, turret, overlord gatling. |
| `up.napalm` | Fire damage | Black Napalm | Flame tank, inferno, MiG. |
| `up.uranium` | Tank damage | Uranium Shells | Needs nuke silo. |
| `up.nuke_engine` | Tank speed | Nuclear Tanks | Needs nuke silo. Explodes messier. |
| `up.mines` | Building mines | Land Mines | |
| `up.neutron_mines` | Crew-kill mines | Neutron Mines | ZH. |
| `up.mig_armor` | Jet HP | MiG Armor | |
| `up.subliminal` | Speaker stronger | Subliminal Messaging | |
| `up.sat_hack` | Stolen intel | Satellite Hack 1/2 | Internet Center. |

### Veil

| id | role | analog | does |
| --- | --- | --- | --- |
| `up.camo` | Still stealth | Camouflage | Rebels (and more on stealth commander). |
| `up.booby` | Anti-capture trap | Booby Traps | ZH. |
| `up.arm_mob` | Mob is real | Arm the Mob | |
| `up.scorp_rocket` | Basic tank missile | Scorpion Rocket | |
| `up.toxin_shells` | Tank linger | Toxin Shells | |
| `up.anthrax` | Toxin tier | Anthrax Beta / Gamma | Gamma is toxin commander. |
| `up.ap_bullets` | Gun damage | AP Bullets | Rifle, mob, technical, quad, hero. |
| `up.ap_rockets` | Rocket damage | AP Rockets | Rocket inf, buggy, stinger, scorp rockets. |
| `up.junk_repair` | Field regen | Junk Repair | Vehicles. |
| `up.buggy_ammo` | More rockets | Buggy Ammo | |
| `up.shoes` | Worker speed | Worker Shoes | |
| `up.fortify` | Building HP | Fortified Structure | |
| `up.van_scan` | Pulse reveal | Radar Van Scan | |
| `up.camo_net` | Building stealth | Camo Netting | Stealth commander. |
| `up.demo_kit` | Everything explodes | Demolition | Demo commander. Units can self-detonate. |

---

## Commander powers

Unlock / passive / click. Vanilla skirmish tree. Specialists rearrange; see loadout.

### Shared

| id | kind | analog | does |
| --- | --- | --- | --- |
| `pow.repair` | click 1–3 | Emergency Repair | Area vehicle heal. |

### Aegis

| id | kind | analog | does |
| --- | --- | --- | --- |
| `pow.unlock_elite_tank` | unlock | Paladin | |
| `pow.unlock_stealth_air` | unlock | Stealth Fighter | |
| `pow.unlock_sniper` | unlock | Pathfinder | |
| `pow.drone` | click | Spy Drone | Stealthed detector, stays. |
| `pow.paradrop` | click 1–3 | Para Drop | Rifle from the sky. |
| `pow.a10` | click 1–3 | A-10 Strike | Precision strike. |
| `pow.fuel_air` | click | Fuel Air Bomb | Structure / clump. |
| `pow.leaflet` | click | Leaflet Drop | ZH. Disable units. |
| `pow.spectre` | click 1–3 | Spectre Gunship | ZH. Loiter delete. |
| `pow.moab` | upgrade | MOAB | Fatter fuel-air. |
| `pow.carpet.aegis` | click | Carpet Bomb | Air commander, from tech. |
| `pow.satellite` | building click | Spy Satellite | On command. Short reveal. Not a point spend. |

### Forge

| id | kind | analog | does |
| --- | --- | --- | --- |
| `pow.vet_rifle` | passive | Red Guard Training | Rifle come veteran. |
| `pow.vet_arty` | passive | Artillery Training | |
| `pow.unlock_tacnuke` | unlock | Nuke Cannon | |
| `pow.mines_air` | click | Cluster Mines | |
| `pow.barrage` | click 1–3 | Artillery Barrage | Unstoppable off-map. |
| `pow.cash_hack` | click 1–3 | Cash Hack | Steal from a visible drop-off. |
| `pow.emp` | click | EMP Pulse | Vehicles and buildings off. Planes fall. |
| `pow.carpet.forge` | click | Carpet Bomb | ZH. |
| `pow.frenzy` | click 1–3 | Frenzy | ZH. Temporary combat buff. |
| `pow.nuke_carpet` | click | Nuke Bomber | Nuke commander. Replaces carpet. |

### Veil

| id | kind | analog | does |
| --- | --- | --- | --- |
| `pow.unlock_scud` | unlock | Scud Launcher | |
| `pow.unlock_marauder` | unlock | Marauder | |
| `pow.vet_technical` | passive | Technical Training | |
| `pow.unlock_hijack` | unlock | Hijacker | |
| `pow.ambush` | click 1–3 | Rebel Ambush | Spawn rifle anywhere you see. Toxin: toxin rifle. |
| `pow.bounty` | passive 1–3 | Cash Bounty | Percent of kill value. |
| `pow.anthrax_bomb` | click | Anthrax Bomb | Infantry delete + linger. |
| `pow.gps` | click | GPS Scrambler | ZH. Cloak a group. Stealth commander: earlier / faster. |
| `pow.sneak` | click | Sneak Attack | ZH. Drop a tunnel entrance. |

---

## Loadout grid (12 playable)

Vanilla = all vanilla roles for that side. Specialist = vanilla **plus** the delta.

### Aegis

| id | doctrine | add / replace | drop / pay |
| --- | --- | --- | --- |
| `aegis.vanilla` | Quality air | — | — |
| `aegis.air` | Air | `air.king`, `air.combat_chinook`, stealth `air.heli`, `pow.carpet.aegis`, cheaper air | Weak ground. Lives on pads. |
| `aegis.laser` | Laser | `armor.laser`, laser `def.patriot`, cheaper `veh.avenger`, fatter fusion | No `armor.basic`, `armor.elite`, `arty.missile` |
| `aegis.super` | Beam | Cheap `sw.beam`, `air.alpha`, EMP `def.patriot`, advanced rods | No Crusader/Paladin line. Vehicles cost more. Turtle then beam. |

### Forge

| id | doctrine | add / replace | drop / pay |
| --- | --- | --- | --- |
| `forge.vanilla` | Mass armor | — | — |
| `forge.tank` | Tanks | Cheap/strong tanks, `armor.emperor` | No `arty.napalm`, no `arty.tacnuke`. Air costs more. |
| `forge.infantry` | Infantry | `inf.minigun`, bigger bunkers, infantry powers | Weak armor line. |
| `forge.nuke` | Nuclear | Nuke shells / nuke MiG from the start of his tree, `pow.nuke_carpet` | Dirty linger is the plan. |

### Veil

| id | doctrine | add / replace | drop / pay |
| --- | --- | --- | --- |
| `veil.vanilla` | Guerrilla | — | — |
| `veil.toxin` | Linger | `inf.toxin_rifle`, toxin tunnels, anthrax already on, fatter storm | No HE bomb truck, no hijack, no saboteur, no camo line. Vehicles cost more. |
| `veil.demo` | Explosives | Fatter bombs, `up.demo_kit` (self-detonate) | Less toxin / stealth. |
| `veil.stealth` | Camo | Camo rifle from minute one, cloaked hijack from start, earlier GPS | No tanks, no scuds. If seen, dead. |

### Not playable (later AI)

| id | doctrine | notes |
| --- | --- | --- |
| `boss.mix` | All three kits | Challenge boss analog. Cheats by mixing Aegis/Forge/Veil toys. |

---

## Slice-1 minimum (not a new scope — a filter)

When we build the first match, implement **mechanics 1–11 and 14–15** plus this role subset:

`build.dozer`, `eco.dropoff`, `eco.chinook`, `power.fusion`, `prod.command`, `prod.barracks`, `prod.factory`, `inf.rifle`, `inf.rocket`, `armor.basic`, `veh.scout_gun`, `def.patriot`, `map.dock`, `map.garrison`, `up.capture`.

Win: destroy enemy `prod.command` and remaining buildings. Computer plays the same subset.

Everything else stays in this catalog until that match is fun.
