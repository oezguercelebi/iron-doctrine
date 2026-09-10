# Shared mechanics

How *Generals* and *Zero Hour* actually play. Unit stats, build times, and map layouts are **out of this document**. Next pass.

## Match

Real-time. Fog of war. You start with a command structure and a builder (USA/China: dozer; GLA: workers).

**Win:** destroy the opponent's buildings (skirmish / Challenge) or complete the mission script (campaign). Losing your army is recoverable; losing production and the command structure usually is not.

**Players:** 1 vs AI, or up to 8 in custom multiplayer.

## Construction

- Buildings are placed in the world by a **builder unit**, not spawned from a yard.
- USA and China: expensive dozers, one building at a time per dozer, dozers also repair and clear mines.
- GLA: cheap **workers** that both build and harvest. Many workers means many buildings at once.
- You may build **anywhere** you have space and (for USA/China) power to run the result. Forward bases are legal and expected.
- Rally points on production buildings. Queues exist.

This is the single biggest mechanical break from Tiberium / Red Alert.

## Economy

### Primary: supply docks

- Neutral, indestructible caches. Default **$30,000** per dock; missions and maps vary. Smaller **supply piles** exist.
- Only **one gatherer loads at a time** at a dock. Extra gatherers wait or take another pile.
- Gatherer returns to *your* drop-off (Supply Center / Supply Stash). Distance is the tax.
- Docks empty. The mid-game is "expand or go secondary."

**Gatherer personality (the faction is the economy):**

| Side | Gatherer | Per trip (vanilla) | Note |
| --- | --- | --- | --- |
| USA | Chinook | $600 | Fast, ignores terrain, dies to AA, also a transport |
| China | Supply truck | $300 | Comes with the building; extra trucks are cheap |
| GLA | Worker | small, slow | Same unit that builds; shoes upgrade helps |

USA takes more per trip; China and GLA take more per minute if they spam gatherers.

### Secondary income (finite docks force this)

- **Oil derricks** (neutral). Capture. Passive cash.
- USA: **Supply Drop Zone** — timed air drop of crates. Pauses if you are low on power.
- China: **Hackers** (and later Internet Center) — sit and print money. Fragile.
- GLA: **Black Market** — upgrades *and* a trickle. **Salvage** crates from wrecks. **Cash bounty** power (percent of kill value). Stealing buildings and selling them.

Capturing the enemy drop-off and draining it (Black Lotus, later saboteurs) is a real strategy.

## Power

- USA and China: buildings need a power pool. Low power: radar and defenses go dark, some production / drop-zone clocks stall.
- USA: Cold Fusion. Control-rod upgrade doubles a plant.
- China: Nuclear reactor. Explodes nastily if sold or killed.
- **GLA: no power.** That is a faction identity, not a missing feature.

## Population

Generals does **not** use C&C4-style command points. Army size is limited by cash, build time, and (softly) by how many units you can micro. There is an engine unit cap in the original, high enough that economy is the real cap. We should pick an explicit cap when we simulate; it is not a designed "command point" layer in the source.

## Combat loop

Rock-paper-scissors with garrison and air as extra axes.

- Rifle infantry beat other infantry and lose to vehicles that crush or splash them.
- Rocket infantry beat armor and air, lose to anti-infantry.
- Tanks beat vehicles and buildings, lose to rockets, air, and being kited.
- Anti-air exists as a unit *and* as a building. USA lives and dies on this vs GLA quads / China gatling.
- Artillery outranges defenses and is fragile.
- Air: USA is an air faction; China has a thin air force (MiGs, later Helix); GLA has **no air force** and compensates with the best organic AA and stealth.

Crushing infantry with tanks is real. So is garrison.

### Garrison

- Civilian buildings hold a squad (about 8 in Generals, about 10 in Zero Hour; large buildings hold more).
- Occupants shoot; the building takes the hits until it is too damaged, then they spill out hurt.
- Clearing a garrison needs flame, toxin, microwave, flashbangs, or a Chinook combat drop — not "shoot the walls forever."
- Purpose-built garrison: USA firebase, China bunker, GLA palace.
- Snipers (Pathfinder, Jarmen Kell) can sit in a building without flipping its team color.

Maps are designed around clusters of garrisonable houses on docks and chokes.

### Veterancy

Three ranks: Veteran, Elite, Heroic.

Earned from value of kills (and some capture / hack actions).

Each rank: more rate of fire and damage; health bumps; Elite and Heroic **self-heal**. Heroic often gets a visible tracer change.

Some powers and some Zero Hour generals **train units already veteran**.

USA Zero Hour: Advanced Training — units rank twice as fast.

### Upgrades

Researched at buildings, applied to units already in the field (armor-piercing, napalm, composite armor, toxin shells, and so on). Faction identity is as much the upgrade list as the roster.

## Generals promotion (the commander bar)

Killing units and buildings fills **your** XP, not just the unit's.

Ranks in skirmish / multiplayer: 1-star, 3-star, 5-star. You get a small number of **skill points**. There are more powers than points, so the bar is a loadout inside the match.

Powers are one of:

- **Unlock** a unit (Paladin, Stealth Fighter, Pathfinder, Nuke Cannon, Marauder, Scud, Hijacker).
- **Passive** (train this unit as veteran; cash bounty on kills).
- **Click on the map** with a cooldown (spy drone, para-drop, A-10, artillery barrage, cash hack, rebel ambush, EMP, fuel-air bomb, anthrax bomb, …).

Shared across sides: **Emergency Repair** (area heal on vehicles), in three strengths.

USA also has a **Spy Satellite** on the command center (short reveal, short cooldown) without spending a point.

Campaign trees differ from skirmish trees. Our game should pick one tree and stick to it.

Zero Hour adds more click-powers (Spectre, leaflets, carpet bomb, frenzy, GPS scrambler, sneak attack) and rearranges them per specialist.

## Superweapons

A building. Visible on the map. Long build, long charge, announced to the opponent.

| Side | Weapon | Character |
| --- | --- | --- |
| USA | Particle Cannon | Steerable beam. Precision. Can finish GLA holes. |
| China | Nuclear Missile | Huge blast + radiation. |
| GLA | Scud Storm | Nine missiles, explosive and/or toxin. |

Killing the building resets the clock. Racing two superweapons is a late-game mode of play.

USA Strategy Center **battle plans** (bombardment / hold the line / search and destroy) are a separate global buff, not a superweapon.

## Neutral map objects

Capture with upgraded infantry (channel, not instant):

- Oil derrick — money
- Oil refinery — cheaper vehicles
- Hospital — infantry heal
- Some maps: extra crates, tech buildings

Civilians exist. Killing them is possible; the German cut removed them.

## Stealth, detection, intel

- Stealth: snipers, Burton, Lotus, GLA camouflage, bomb-truck disguise, fake buildings, Kassad almost everything.
- Detection: USA drones / pathfinders / patrols, China troop crawler, GLA radar van, listening outpost (ZH).
- USA starts with radar. China researches it on the command center. GLA builds a Radar Van or they have no minimap.

A match without detection vs GLA is a different game.

## GLA hole (faction rule that is also a system)

Destroyed GLA buildings leave a **hole**. If the hole is not crushed, the building rebuilds. Particle Cannon and some other tools can delete the hole. "I shot the barracks" is not enough.

## Tunnels (GLA)

Tunnel Network is a defense *and* a global subway. Enter any node, exit any other. Zero Hour **Sneak Attack** drops a new unarmed entrance anywhere. This is how GLA ignores the map.

## Heroes

One at a time per side:

- USA: Colonel Burton — stealth, knife, demo charges.
- China: Black Lotus — steal buildings, disable vehicles, steal cash.
- GLA: Jarmen Kell — sniper, can shoot the driver out of a vehicle for capture.

## What we will specify later

- Every unit's role, counters, cost, and special ability
- Every building's prerequisites and power draw
- Every specialist's buff/nerf table as *our* commanders
- Map language: spawn, docks, garrison belts, high ground, water
- Exact XP thresholds, point budgets, cooldowns
- Engine unit cap
