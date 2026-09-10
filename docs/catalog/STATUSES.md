# Statuses and leftover entities

Flags and fields that are not units. If it can sit on the map or on a body, it is here.

---

## Flags on a unit or building

| id | on | analog | does | cleared by |
| --- | --- | --- | --- | --- |
| `st.stealth` | unit / building | Camo, still-sniper, GPS | Invisible until detected or it fires | Detector, attack, some abilities |
| `st.detected` | stealthed thing | Spotted | Visible to that enemy while the detector lives / lasts | Detector dies |
| `st.disguise` | `deceive.bomb_truck` | Disguise | Looks like another vehicle | Detector, attack, detonate |
| `st.garrisoned` | infantry | Inside | Building takes hits. Occupant rules in MECHANICS 8 | Exit, building too damaged |
| `st.packed` | `arty.tacnuke` | Packed | Can move, cannot fire | Unpack |
| `st.capturing` | infantry + building | Channel | Interruptible | Damage / move / death |
| `st.disabled` | vehicle / building | EMP, leaflet, microwave, lotus, saboteur | Cannot act. Planes fall | Timer, channel break |
| `st.jammed` | missiles / vehicles near ECM | ECM | Missiles miss. Vehicles stall | Leave radius |
| `st.horde` | Forge rifle / rocket / basic tank | Horde | 5+ in a clump: faster shots | Split the clump |
| `st.frenzy` | Forge units | Frenzy power | Short combat buff | Timer |
| `st.vet.1` `st.vet.2` `st.vet.3` | units | Veteran / Elite / Heroic | See MECHANICS 9. Elite+ self-heal | — |
| `st.low_power` | Aegis / Forge player | Brownout | Radar off, defenses off, some clocks stall | Build / repair plants |
| `st.overcharge` | `power.reactor` | Overcharge | Extra power, then boom | Toggle off |
| `st.plan.damage` `st.plan.armor` `st.plan.vision` | Aegis army | Battle plan | One at a time | Switch |
| `st.booby` | building | Booby trap | Next capture kills the capper | Clear mines |
| `st.fake` | `def.fake` | Fake | Looks like a real building | Detector, upgrade-to-real |
| `st.hole_pending` | Veil building wreck | GLA hole | Will rebuild unless crushed | Crush / beam |

---

## Linger fields (area on the map)

| id | analog | hurts | cleaned by | notes |
| --- | --- | --- | --- | --- |
| `field.fire` | Napalm / firestorm | Infantry, light, linger | Time | Stacks into a firestorm |
| `field.toxin` | Anthrax / spray | Infantry, linger | `ab.cleanse` | Toxin commander fattens |
| `field.rad` | Nuke blast / shells | Infantry, linger | `ab.cleanse` | |
| `field.microwave` | Microwave bubble | Infantry | Tank death | Not a puddle; follows the tank |
| `field.leaflet` | Leaflet drop | Disable units | Timer | |
| `field.emp` | EMP pulse | Disable vehicles + buildings; planes die | Timer | |

---

## Map entities that are not “units” or “buildings”

| id | analog | interact | notes |
| --- | --- | --- | --- |
| `ent.hole` | GLA hole | Crush, beam, ignore | Rebuilds the building if left. **Must exist** or Veil identity is fake. |
| `ent.salvage` | Wreck crate | `ab.salvage` pickup | Dropped by dead vehicles. Two ranks. |
| `ent.mine` | Land mine / neutron | Detonate on enemy | Forge rings. Neutron kills crews. |
| `ent.trap` | Demo trap | Detonate | Stealthed. See DETECTION. |
| `ent.drop_crate` | Supply drop crate | Pick up cash | From `eco.drop_pad` |
| `ent.wreck_bus` | Battle Bus husk | Still a bunker | Until crushed |
| `ent.empty_hull` | Driver-sniped vehicle | Infantry enter to steal | From `ab.snipe_driver` / neutron |

---

## Player-level clocks

| id | analog | notes |
| --- | --- | --- |
| `clk.power_charge` | Superweapon timer | Visible. Killing the building resets it. Announced to enemies. |
| `clk.pow.*` | Commander power cooldown | Per power. |
| `clk.drop_pad` | Supply drop | Pauses on `st.low_power`. |
| `clk.satellite` | Spy satellite | Short. |
| `clk.promote` | Commander XP bar | 1 / 3 / 5 star. |

---

## Slice-1 statuses

`st.capturing`, `st.garrisoned`, `st.low_power`. No linger fields, no hole, no salvage until those factions exist.
