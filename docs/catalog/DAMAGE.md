# Damage and armor

Qualitative matrix. Not DPS. If a cell is “poor”, that role should lose the even fight.

---

## Armor classes

| id | worn by |
| --- | --- |
| `arm.infantry` | Rifle, rocket, suicide, hacker, worker, hero bodies |
| `arm.light` | Humvee, technical, quad, crawler, ambulance, buggy, cycle, radar van |
| `arm.tank` | Basic / elite / marauder / flame / gatling / mammoth |
| `arm.arty` | Tomahawk, inferno, nuke cannon, scud — tank-ish but fragile in practice |
| `arm.air` | All aircraft |
| `arm.structure` | Buildings |
| `arm.garrison` | Occupied civilian / bunker (hits the box, not the men, until it breaks) |
| `arm.hole` | `ent.hole` — only crush, beam, or dedicated clear |

Heroes sit in `arm.infantry` but resist crush.

---

## Damage kinds

| id | typical source |
| --- | --- |
| `dmg.small` | Rifle, gatling, technical, quad vs ground |
| `dmg.cannon` | Tanks, firebase |
| `dmg.rocket` | Rocket infantry, stinger, buggy, fighter missiles, TOW |
| `dmg.explosive` | Artillery, bomb truck HE, fuel-air, demo |
| `dmg.flame` | Flame tank, inferno, MiG napalm |
| `dmg.toxin` | Spray, anthrax, toxin shells |
| `dmg.rad` | Nuke blast / shells |
| `dmg.laser` | Laser tank, laser turret, avenger, paladin PDL (as a *defend*) |
| `dmg.emp` | EMP pulse, EMP patriot |
| `dmg.sniper` | Pathfinder, Kell vs infantry |
| `dmg.crush` | Tank drive-over |
| `dmg.microwave` | Microwave bubble / beam |
| `dmg.intercept` | PDL, stinger, RPG vs *missiles in flight* |

---

## Matrix

**good** = this is the point of the weapon. **ok** = works. **poor** = even fight is a loss. **none** = no effect.

| vs → | inf | light | tank | arty | air | structure | garrison |
| --- | --- | --- | --- | --- | --- | --- | --- |
| small | good | ok | poor | poor | poor* | poor | poor |
| cannon | ok (crush) | good | good | good | none | good | poor |
| rocket | poor | ok | good | good | good | good | poor |
| explosive | good | good | ok | ok | none | good | ok |
| flame | good | ok | poor | poor | none | ok | **good** |
| toxin | good | ok | poor | poor | none | poor | **good** |
| rad | good | ok | ok | ok | none | ok | ok |
| laser | ok | good | good | good | good | ok | poor |
| emp | none | disable | disable | disable | **kills** | disable | none |
| sniper | **good** | none | driver only (Kell) | none | none | none | none (hidden) |
| crush | good | none | none | none | none | none | none |
| microwave | **good** | poor | poor | poor | none | **offline** | **good** |

\*quad / gatling / patriot small-or-rocket vs air is **good** — those roles are AA, not “rifle vs jet”.

---

## Intercept

These damage kinds can be **shot down** in flight: `dmg.rocket` from Tomahawk / Scud / some fighter missiles.

Eaters: `ab.pdl`, stinger, RPG (some), avenger, ECM (miss, not eat).

Beam superweapon and nuke ballistic are **not** interceptable. That is the point.

---

## Slice-1

Implement `dmg.small`, `dmg.cannon`, `dmg.rocket`, `arm.infantry`, `arm.light`, `arm.tank`, `arm.structure`, `arm.garrison`, `dmg.crush`. No linger, EMP, laser, microwave.
