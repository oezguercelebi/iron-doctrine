# Factions

Our doctrines. Not the source roster.

A side is a **logistics story** plus a **combat story**. If you can swap the labels and not notice, the faction failed.

Specialists **replace or drop** vanilla roles. They are not a thirteenth faction. Grid: [ROLES.md](ROLES.md). Prereqs: [TECH_TREE.md](TECH_TREE.md). Names in this file are working labels; **ids** are stable. Do not use source commander names.

---

## Shared skeleton

Every playable loadout has:

- A builder
- A supply drop-off
- Barracks, a vehicle plant, and either air **or** a stated substitute (Veil: no air)
- A tech structure
- A superweapon
- One hero
- Capture-capable basic infantry (after `up.capture`, or a hero that captures)
- Rocket infantry
- A basic tank (Veil’s is weak until upgrades; stealth loadout **drops** tanks)
- A promotion tree with more powers than points

Personality is what happens when you **break** the skeleton (no air, no power, no tanks, only air, only linger).

---

## Aegis — quality, air, intel

**id:** `aegis.vanilla` and the three Aegis specialists.

**Logistics.** Slowest eco for the cost of the army. Flying gatherer (`eco.chinook`): fat load, ignores ground path, dies to AA. Power is tight; plants are clean. Units are expensive.

**Combat.** Best air. Ground combat vehicles hang **one** drone. Rifle can flashbang and combat-drop. Sniper deletes infantry. Elite tank eats missiles. Beam superweapon is steerable and deletes Veil holes.

**Hole.** No mass. Lose Chinooks and the money stops. Lose the sky and air loadouts fold. Early game is “get the base up before someone walks in.”

**Hero:** `hero.demo_stealth`. **Tech:** `prod.tech.aegis` (battle plans). **Radar:** on `prod.command` from minute zero. **Satellite:** building click, not a point.

### Loadouts

| id | doctrine | gets | pays |
| --- | --- | --- | --- |
| `aegis.vanilla` | Quality air | Vanilla Aegis | — |
| `aegis.air` | Air | `air.king`, `air.combat_chinook`, stealth `air.heli`, `pow.carpet.aegis`, cheaper air | Weak ground. Lives on pads. |
| `aegis.laser` | Laser | `armor.laser`, laser `def.patriot`, cheaper `veh.avenger`, fatter fusion | No `armor.basic`, `armor.elite`, `arty.missile` |
| `aegis.super` | Beam | Cheap `sw.beam`, `air.alpha`, EMP `def.patriot`, advanced rods | No Crusader/Paladin line. Vehicles cost more. Turtle then beam. |

---

## Forge — mass, armor, fire, nukes

**id:** `forge.vanilla` and the three Forge specialists.

**Logistics.** Cheap units, extra gatherer trucks, sit-hackers / internet as a money farm. Plants are bombs if they pop. Command has **no radar until** `ab.radar_upgrade`.

**Combat.** **Horde:** 5+ rifle, rocket, or basic tanks shoot faster (`up.horde` innate; `up.nationalism` fattens). Mammoth is the superheavy with one add-on. Napalm stacks into firestorm. Mines ring buildings. Speaker heals. EMP and the nuke silo are the late hammer.

**Hole.** Slow ground. Thin air. You win a long fight and lose a raid.

**Hero:** `hero.capture`. **Tech:** `prod.tech.forge`.

### Loadouts

| id | doctrine | gets | pays |
| --- | --- | --- | --- |
| `forge.vanilla` | Mass armor | Vanilla Forge | — |
| `forge.tank` | Tanks | Cheap/strong tanks, `armor.emperor` | No `arty.napalm`, no `arty.tacnuke`. Air costs more. |
| `forge.infantry` | Infantry | `inf.minigun`, bigger bunkers, infantry powers | Weak armor line. |
| `forge.nuke` | Nuclear | Nuke shells / nuke MiG from the start of the tree, `pow.nuke_carpet` | Dirty linger is the plan. |

---

## Veil — guerrilla, stealth, salvage, no grid

**id:** `veil.vanilla` and the three Veil specialists.

**Logistics.** Workers build *and* harvest. **No power.** Black market trickles cash and sells upgrades. Salvage crates grow Technical / Quad / Marauder. Cash-bounty power pays for kills. Fake buildings waste shots. Tone: professional irregulars, not a caricature.

**Combat.** Hit-and-run. Tunnel network is a map graph. Traps, disguised bomb truck, hijack, driver-snipe. Quad is the AA that makes Aegis air die. Storm is the superweapon. **Hole:** unless you crush `ent.hole`, the building comes back.

**Hole (faction).** No air. Thin armor unless scavenged. Direct even fight against mammoths or elite tanks is a loss. You need numbers, angles, or theft.

**Hero:** `hero.sniper`. **Tech:** `prod.tech.veil` (garrison 5). **Radar:** `veh.radar`. Kill the van, lose the map.

### Loadouts

| id | doctrine | gets | pays |
| --- | --- | --- | --- |
| `veil.vanilla` | Guerrilla | Vanilla Veil | — |
| `veil.toxin` | Linger | `inf.toxin_rifle`, toxin tunnels, anthrax already on, fatter storm | No HE bomb truck, no hijack, no saboteur, no camo line. Vehicles cost more. |
| `veil.demo` | Explosives | Fatter bombs, `up.demo_kit` (self-detonate) | Less toxin / stealth. |
| `veil.stealth` | Camo | Camo rifle from minute one, cloaked hijack from start, earlier GPS | No tanks, no scuds. If seen, dead. |

---

## Not playable

| id | notes |
| --- | --- |
| `boss.mix` | Challenge boss analog. Mixes Aegis / Forge / Veil toys. AI-only, later. |

Specialist **product** names are not chosen. Do not fill them with analog names.

---

## Slice-1

`aegis.vanilla` vs `aegis.vanilla`. Other loadouts stay in this file.
