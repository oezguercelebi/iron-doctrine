# Detection

Who is invisible, who sees them. Vs Veil this is the game.

---

## Stealth kinds

| id | analog | hidden while | broken by |
| --- | --- | --- | --- |
| `stealth.still` | Pathfinder, Kell, Lotus, Burton, Hijacker | Not moving (Burton often also moving) | Move (some), fire, detector |
| `stealth.camo` | Camo rifle, GPS group | Not firing | Fire, detector |
| `stealth.air` | Stealth fighter, stealth heli | Not attacking | Attack run, detector AA |
| `stealth.building` | Camo net, stealthed listening post | Always until it fires | Detector, attack |
| `stealth.fake` | Fake building | Looks like a real building | Detector, upgrade-to-real, selling |
| `stealth.disguise` | Bomb truck | Looks like the copied vehicle | Detector, attack, wrong-team tell |
| `stealth.trap` | Demo trap, some mines | Always | Detector, then shoot |
| `stealth.drone` | Spy drone, sentry | Always until it fires (if gunned) | Detector |

`ent.hole` is **visible**. You have to choose to crush it.

---

## Detectors

| id | analog | sees | does not see |
| --- | --- | --- | --- |
| `det.sentry` | Sentry drone | still, camo, disguise, drone, trap | stealthed air at altitude unless in range |
| `det.spy_drone` | Spy drone | still, camo, disguise, drone | traps at the edge of vision — treat as yes in radius |
| `det.pathfinder` | Pathfinder | still infantry, camo infantry | disguised vehicles, traps, stealthed buildings (weak) |
| `det.crawler` | Troop crawler | still, camo, disguise, trap, drone | stealthed air |
| `det.listen` | Listening outpost | still, camo, disguise, trap, building, drone | — |
| `det.van` | Radar van | still, camo, disguise, trap, drone | stealthed air unless scan |
| `det.scan` | Van scan / satellite / sat-hack | everything in the pulse | — |
| `det.addon_gatling` | Overlord/helix gatling | trap, nearby stealth | air stealth |
| `det.attack` | Any attacker | itself, while shooting | — |
| `det.proximity` | Bumping a trap | that trap | — |

Aegis satellite (`ab.satellite`) is a **reveal**, not a standing detector.

---

## Matrix (need a detector?)

| hidden kind | rifle | tank | AA turret | listed detector | satellite pulse |
| --- | --- | --- | --- | --- | --- |
| `stealth.still` | no | no | no | yes | yes |
| `stealth.camo` | no | no | no | yes | yes |
| `stealth.air` | no | no | only if attacking | detector AA / pulse | yes |
| `stealth.building` | no | no | no | yes | yes |
| `stealth.fake` | looks real | looks real | looks real | yes (or ignore) | maybe |
| `stealth.disguise` | looks like friend/foe copy | same | same | yes | yes |
| `stealth.trap` | boom | boom | boom | yes, then shoot | pulse may show |
| `stealth.drone` | no | no | if it shoots | yes | yes |

**no** = ordinary unit does not see it.

---

## Faction kit (vanilla)

| side | built-in detect | hole |
| --- | --- | --- |
| Aegis | Satellite, later sentry / pathfinder / scout drone | Slow to get standing detect. Spy drone power is the opener. |
| Forge | Troop crawler (early), listening outpost (ZH) | Blind minimap until radar upgrade. |
| Veil | Radar van | No van = no minimap *and* weak detect. |

---

## Slice-1

Fog of war + shroud only. No stealth roles in the first match. Detection code can wait until Veil or Pathfinder ships.
