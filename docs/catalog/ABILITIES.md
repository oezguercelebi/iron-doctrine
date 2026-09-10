# Abilities

Click, toggle, channel, add-on. Not the unit itself — what it *does* besides shoot and move.

**kind:** `click-ground` `click-unit` `click-self` `toggle` `channel` `deploy` `addon` `passive` `enter`

Owner ids match [ROLES.md](ROLES.md). Commander map-clicks live in ROLES (`pow.*`). This file is **unit and building** specials.

---

## Builders & economy

| id | owner | kind | analog | does | cancelled by |
| --- | --- | --- | --- | --- | --- |
| `ab.build` | `build.dozer`, `build.worker` | click-ground | Build | Ghost + place a building | Death, invalid terrain |
| `ab.repair` | same | click-unit | Repair | Repair own/ally building | Death, full HP |
| `ab.clear_mines` | same | click-ground | Clear mines | Remove mines / traps / booby | Death |
| `ab.gather` | gatherers, `build.worker` | passive | Collect | Auto-load dock, return to drop-off | Death, empty dock |
| `ab.combat_drop` | `eco.chinook` with `inf.rifle` | click-building | Combat Drop | Rappel into a garrison; take it | AA, empty hold |
| `ab.hack_print` | `eco.hacker` | click-self | Hack Internet | Sit, print cash. Faster with vet | Move, death, splash |
| `ab.hack_disable` | `eco.hacker` | channel building | Disable | Building offline while channelled | Interrupt, death |

---

## Infantry & heroes

| id | owner | kind | analog | does | cancelled by |
| --- | --- | --- | --- | --- | --- |
| `ab.capture` | rifle after `up.capture` | channel building | Capture | Flip building to you | Interrupt, death, `up.booby` |
| `ab.flashbang` | Aegis `inf.rifle` | toggle | Flashbangs | Gun ↔ grenade. Grenade clears garrison / mob | — |
| `ab.laser_lock` | Aegis `inf.rocket` | click-unit | Laser lock | Slow, high-accuracy rocket on one target | Move, death |
| `ab.tnt` | Forge `inf.rocket` | click-unit | TNT | Timed charge on a vehicle | Death of planter |
| `ab.car_bomb` | `inf.suicide` | enter `map.car` | Capture vehicle | Car becomes a faster bomb | Death |
| `ab.hijack` | `inf.hijack` | click-unit | Hijack | Steal vehicle (not high-vet). Steal dozer = their build kit | Detection, death |
| `ab.climb` | `inf.saboteur` | passive | Cliff climb | Ignores some cliff pathing | — |
| `ab.sabotage` | `inf.saboteur` | channel building | Infiltrate | Power down. On command: reset their powers | Detection, death |
| `ab.pilot_enter` | `inf.pilot` | click-unit | Enter vehicle | Pass stored vet onto a friendly vehicle | Death |
| `ab.knife` | `hero.demo_stealth` | click-unit | Knife | Silent infantry kill | Detection |
| `ab.charge_timed` | `hero.demo_stealth` | click-building | Timed charge | Explodes after a delay | Disarm / death of building |
| `ab.charge_remote` | `hero.demo_stealth` | click-building + click-self | Remote charge | Detonate on command | Same |
| `ab.lotus_capture` | `hero.capture` | channel building | Capture | Faster capture, stealthed | Detection |
| `ab.lotus_disable` | `hero.capture` | channel vehicle | Vehicle hack | Vehicle frozen | Interrupt |
| `ab.lotus_cash` | `hero.capture` | channel drop-off | Cash hack | Steal from enemy supply | Interrupt |
| `ab.snipe_driver` | `hero.sniper` | click-unit | Pilot snipe | Empties a vehicle. Any infantry can then steal it | Reload, death |
| `ab.booby` | Veil rifle after `up.booby` | click-building | Booby trap | Next capture attempt kills the capper | Clear mines |

---

## Vehicles, armor, artillery

| id | owner | kind | analog | does | cancelled by |
| --- | --- | --- | --- | --- | --- |
| `ab.drone_hang` | Aegis combat vehicles | addon (one) | Battle / Scout / Hellfire | See add-ons below | Vehicle death |
| `ab.cleanse` | `veh.ambulance` | passive / click | Clear toxin | Remove toxin/rad in radius. Heal infantry. ZH: repair vehicles | Death |
| `ab.salvage` | `veh.technical`, `veh.quad`, `armor.marauder` | enter crate | Salvage | Two crates grow the gun | Death |
| `ab.disguise` | `deceive.bomb_truck` | click-unit type | Disguise | Look like that vehicle | Detector, attack, detonate |
| `ab.payload` | `deceive.bomb_truck` | toggle | HE / bio | Blast vs linger. Toxin commander: bio only. Demo: HE | — |
| `ab.detonate` | bomb, suicide, demo-kit units | click-self | Detonate | Explode now | Death first |
| `ab.van_scan` | `veh.radar` after `up.van_scan` | click-ground | Radar scan | Pulse reveal | Death of van |
| `ab.cycle_mount` | `veh.cycle` + infantry | enter | Mount | Bike inherits rider weapon | Death |
| `ab.firewall` | `armor.flame` | click-ground | Fire wall | Line of linger flame | — |
| `ab.microwave` | `veh.microwave` | passive | Microwave | Infantry in bubble die. Buildings pointed at go offline | Death |
| `ab.ecm` | `veh.ecm` | passive | Jammer | Nearby missiles miss. Nearby vehicles disable | Death |
| `ab.avenger_paint` | `veh.avenger` | passive | Laser paint | Friendly ground in cone shoots faster. Eats missiles | Death |
| `ab.pdl` | `armor.elite` | passive | Point defense laser | Eats incoming missiles | Death |
| `ab.overlord_addon` | `armor.mammoth`, `air.helix` | addon (one) | Bunker / Gatling / Speaker | See add-ons | Death |
| `ab.unpack` | `arty.tacnuke` | deploy | Deploy | Must pack to move, unpack to fire | Stun, death |
| `ab.scud_warhead` | `arty.scud` | toggle | HE / toxin | Buildings vs blobs. Toxin commander: toxin only | — |
| `ab.neutron_shell` | `arty.tacnuke` after research | toggle | Neutron shells | ZH. Kills crews, leaves hulls | — |

---

## Air & buildings

| id | owner | kind | analog | does | cancelled by |
| --- | --- | --- | --- | --- | --- |
| `ab.return_pad` | jets | passive | Reload | Must land to rearm (not heli) | Pad lost → crash when empty |
| `ab.overcharge` | `power.reactor` | toggle | Overcharge | Extra power, then the plant explodes if left on | Toggle off, death |
| `ab.rods` | `power.fusion` | addon | Control rods | This plant makes more power | — |
| `ab.mines` | Forge buildings after `up.mines` | addon | Land mines | Ring of mines. Neutron later | Clear mines |
| `ab.radar_upgrade` | Forge `prod.command` | research | Radar | Minimap on | — |
| `ab.satellite` | Aegis `prod.command` | click-ground | Spy Satellite | Short reveal, short cooldown. Not a point | — |
| `ab.battle_plan` | `prod.tech.aegis` | toggle (one) | Bombardment / Hold / Search | Global: damage / armor / vision | Switch plan |
| `ab.sell` | own building | click-self | Sell | Refund a cut. Veil hole still drops unless noted | — |
| `ab.fake_upgrade` | `def.fake` | click-self | Upgrade to real | Pay the rest, become the real building | Death |
| `ab.tunnel_enter` | Veil units (not mob) | enter `def.tunnel` | Tunnel | Exit any other node | Node dead |
| `ab.beam_steer` | `sw.beam` | click-ground while firing | Particle beam | Drag the beam | Building die, shot ends |
| `ab.drop_crate` | `eco.drop_pad` | passive clock | Supply drop | Plane dumps cash. Stalls if low power | Pad dead, low power |

---

## Add-ons (exactly one)

### Aegis drone (`ab.drone_hang`)

| id | analog | does |
| --- | --- | --- |
| `addon.drone.scout` | Scout drone | Extra vision. Detects stealth. |
| `addon.drone.battle` | Battle drone | Repairs the parent. Small gun. |
| `addon.drone.hellfire` | Hellfire drone | Anti-vehicle missile. |

### Forge mammoth / helix (`ab.overlord_addon`)

| id | analog | does |
| --- | --- | --- |
| `addon.bunker` | Bunker | Holds infantry who shoot. |
| `addon.gatling` | Gatling | Anti-infantry / AA. Detects some stealth. |
| `addon.speaker` | Speaker | Area heal. |

---

## Slice-1 abilities

`ab.build`, `ab.repair`, `ab.gather`, `ab.capture`, `ab.sell`. Nothing else until that match is fun.
