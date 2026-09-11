# Delivery

How a hit **travels**. Damage vs armor is [DAMAGE.md](DAMAGE.md). Who shoots what is the combat join in [ROLES.md](ROLES.md).

If it never leaves the shooter, it is not a missile, and intercept cannot eat it.

---

## Kinds

| id | analog | travels | interceptable |
| --- | --- | --- | --- |
| `del.instant` | Bullets, sniper, gatling, most cannon | Hits this tick. No flight object. | no |
| `del.missile` | Rockets, TOW, patriot, tomahawk, scud, fighter missiles | Projectile in the world. Has a target. | **yes** if [DAMAGE.md](DAMAGE.md) intercept lists that shot |
| `del.beam` | Laser tank, laser turret, particle cannon | Trace. Super beam is steerable (`ab.beam_steer`). | no |
| `del.spray` | Flame, toxin spray, microwave bubble | Cone or radius. May spawn a `field.*`. | no |
| `del.arc` | Unpack artillery, inferno, nuke cannon, firebase howitzer | Ballistic. Often min-range. Often cannot fire packed / on the move. | no (nuke ballistic is the point) |
| `del.drop` | Aurora bomb, supply crate, paradrop, cluster mines | Falls from air / off-map. | no |
| `del.melee` | Crush, knife, suicide contact, trap, mines | Contact. | no |
| `del.channel` | Capture, hack, lotus, saboteur | Not a shot. Interruptible. | n/a |
| `del.pulse` | EMP, leaflet, satellite, van scan | Area, instant. | no |

A role has **one** primary `del` in the combat join. Extra modes (`ab.flashbang`, `ab.scud_warhead`, neutron toggle) stay abilities.

---

## Missiles as objects

While a `del.missile` is in flight it can be:

- **Eaten** by `dmg.intercept` (`ab.pdl`, stinger, some RPG, avenger).
- **Missed** by ECM (`st.jammed`) — not eaten, just fails.
- **Redirected** — do not invent this. Nothing in the catalog retargets a shot except the beam steer, which is `del.beam`.

Not missiles (never eaten):

- Super **beam** and Forge **nuke ballistic**
- `sw.storm` volley (treat as super, not patriot-food)
- Instant, spray, melee, channel, pulse, drop
- Crush

---

## Unpack

`del.arc` + `st.packed`: `arty.tacnuke` must unpack to fire and pack to move. Other artillery that “cannot fire on the move” uses the same idea if a later row says so. Slice-1 has no unpack artillery.

---

## Air targets

`arm.air` is only hittable by roles whose damage matrix is not `none` vs air (rockets, dedicated AA, laser, EMP-kills-planes). Cannon vs air is **none**. That is why a Chinook laughs at tanks and dies to `def.patriot` / `inf.rocket`.

---

## Slice-1

`del.instant` (rifle, tank cannon, scout gun), `del.missile` (rocket infantry, patriot), `del.melee` (crush), `del.channel` (capture). No intercept required until elite tank / avenger / stinger. No beam, spray, arc, drop, pulse.
