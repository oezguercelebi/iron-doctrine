# Tech tree

What must exist before you may build or research a thing. Not costs. Not times.

Specialists inherit vanilla, then apply the loadout grid in [ROLES.md](ROLES.md) (drop / replace).

`start` = builder can place it from minute zero (with space / power rules).

---

## Aegis

```
prod.command          start
power.fusion          start
prod.barracks         start
eco.dropoff           start
def.patriot           power.fusion
prod.factory          eco.dropoff
prod.airfield         prod.factory
prod.tech.aegis       prod.factory
def.firebase          prod.factory          (ZH)
eco.drop_pad          prod.tech.aegis
sw.beam               prod.tech.aegis
prod.detention        prod.tech.aegis       (optional; vanilla Generals)
```

### Units

| unit | from | also needs |
| --- | --- | --- |
| `build.dozer` | `prod.command` | — |
| `eco.chinook` | `eco.dropoff` | — |
| `inf.rifle` `inf.rocket` | `prod.barracks` | — |
| `hero.demo_stealth` | `prod.barracks` | one at a time |
| `inf.sniper` | `prod.barracks` | `pow.unlock_sniper` |
| `armor.basic` `veh.scout_gun` `veh.ambulance` | `prod.factory` | — |
| `armor.elite` | `prod.factory` | `pow.unlock_elite_tank` |
| `arty.missile` `veh.microwave` `veh.avenger` `veh.sentry` | `prod.factory` | `prod.tech.aegis` |
| `air.fighter` `air.heli` | `prod.airfield` | pad free |
| `air.stealth` | `prod.airfield` | `pow.unlock_stealth_air` |
| `air.bomber` | `prod.airfield` | `prod.tech.aegis` |

### Research lives at

| upgrade | at |
| --- | --- |
| `up.capture` `up.flashbang` | `prod.barracks` |
| `up.tow` | `prod.factory` |
| `up.composite` `up.drone_armor` `up.fast_vet` `up.supply_lines` `up.chem_suits` | `prod.tech.aegis` |
| `up.rods` | each `power.fusion` |
| `up.laser_msl` `up.flares` `up.pods` `up.bunker_buster` | `prod.airfield` |
| `up.sentry_gun` | `veh.sentry` or factory |

### Powers (vanilla skirmish)

| rank | spend |
| --- | --- |
| 1-star | `pow.unlock_elite_tank` `pow.unlock_stealth_air` `pow.drone` |
| 3-star | `pow.unlock_sniper` `pow.paradrop` 1–3 `pow.a10` 1–3 `pow.repair` 1–3 |
| 5-star | `pow.fuel_air` (+ ZH: `pow.leaflet` `pow.spectre` `pow.moab`) |

`pow.satellite` is on `prod.command`, not a point.

---

## Forge

```
prod.command          start
power.reactor         start
prod.barracks         start
eco.dropoff           power.reactor
def.gatling           power.reactor
def.bunker            prod.barracks
prod.factory          eco.dropoff
prod.airfield         prod.factory
prod.tech.forge       prod.factory
def.speaker           prod.tech.forge
eco.internet          prod.tech.forge       (ZH)
sw.nuke               prod.tech.forge
```

Radar on `prod.command` is a **research**, not a building.

### Units

| unit | from | also needs |
| --- | --- | --- |
| `build.dozer` | `prod.command` | — |
| `eco.truck` | `eco.dropoff` | first one free |
| `inf.rifle` `inf.rocket` | `prod.barracks` | rifle often pair-built |
| `eco.hacker` | `prod.barracks` | `prod.tech.forge` (or sit at `eco.internet`) |
| `hero.capture` | `prod.barracks` | `prod.tech.forge`, one at a time |
| `armor.basic` `armor.flame` `armor.gatling` `veh.crawler` | `prod.factory` | — |
| `veh.listening` `veh.ecm` | `prod.factory` | ZH |
| `armor.mammoth` `arty.napalm` | `prod.factory` | `prod.tech.forge` |
| `arty.tacnuke` | `prod.factory` | `prod.tech.forge` + `pow.unlock_tacnuke` |
| `air.mig` | `prod.airfield` | — |
| `air.helix` | `prod.airfield` | `prod.tech.forge` |

### Research lives at

| upgrade | at |
| --- | --- |
| `up.capture` | `prod.barracks` |
| `up.chain` `up.napalm` | `prod.factory` |
| `up.nationalism` `up.subliminal` | `prod.tech.forge` |
| `up.mines` | each building |
| `up.neutron_mines` | after `up.mines` |
| `up.uranium` `up.nuke_engine` | `sw.nuke` |
| `up.mig_armor` | `prod.airfield` |
| `up.sat_hack` | `eco.internet` |
| `ab.radar_upgrade` | `prod.command` |

### Powers (vanilla skirmish)

| rank | spend |
| --- | --- |
| 1-star | `pow.vet_rifle` `pow.vet_arty` `pow.unlock_tacnuke` |
| 3-star | `pow.mines_air` `pow.barrage` 1–3 `pow.cash_hack` 1–3 `pow.repair` 1–3 |
| 5-star | `pow.emp` (+ ZH: `pow.carpet.forge` `pow.frenzy`) |

---

## Veil

No power column. Workers build everything.

```
prod.command          start
eco.stash             start
prod.barracks         start
def.tunnel            prod.barracks         (worker-placed defense)
def.stinger           prod.barracks
def.trap              prod.command          (worker)
prod.arms             eco.stash
prod.tech.veil        prod.arms
eco.black_market      prod.tech.veil
sw.storm              prod.tech.veil
def.fake              worker, ZH            (selected fake types)
```

### Units

| unit | from | also needs |
| --- | --- | --- |
| `build.worker` | `prod.command` or `eco.stash` | — |
| `inf.rifle` `inf.rocket` `inf.suicide` | `prod.barracks` | — |
| `inf.mob` | `prod.barracks` | `prod.tech.veil` |
| `inf.hijack` | `prod.barracks` | `prod.tech.veil` + `pow.unlock_hijack` |
| `inf.saboteur` | `prod.barracks` | ZH, `prod.tech.veil` |
| `hero.sniper` | `prod.barracks` | `prod.tech.veil`, one at a time |
| `veh.technical` `armor.basic` `veh.quad` `veh.radar` `veh.toxin_spray` | `prod.arms` | — |
| `veh.cycle` `veh.bus` `deceive.bomb_truck` | `prod.arms` | ZH; bomb truck needs `prod.tech.veil` |
| `armor.marauder` | `prod.arms` | `pow.unlock_marauder` |
| `veh.buggy` `arty.scud` | `prod.arms` | `prod.tech.veil`; scud also `pow.unlock_scud` |

### Research lives at

| upgrade | at |
| --- | --- |
| `up.capture` `up.booby` | `prod.barracks` |
| `up.scorp_rocket` | `prod.arms` |
| `up.camo` `up.arm_mob` `up.toxin_shells` `up.anthrax` | `prod.tech.veil` |
| `up.ap_bullets` `up.ap_rockets` `up.junk_repair` `up.buggy_ammo` `up.shoes` `up.fortify` `up.van_scan` | `eco.black_market` |
| `up.camo_net` | stealth commander buildings |
| `up.demo_kit` | demo commander palace |

### Powers (vanilla skirmish)

| rank | spend |
| --- | --- |
| 1-star | `pow.unlock_scud` `pow.unlock_marauder` `pow.vet_technical` |
| 3-star | `pow.unlock_hijack` `pow.ambush` 1–3 `pow.bounty` 1–3 `pow.repair` 1–3 |
| 5-star | `pow.anthrax_bomb` (+ ZH: `pow.gps` `pow.sneak`) |

---

## Neutral capture (all sides)

Needs `up.capture` (or a hero that captures innately).

`map.oil` `map.refinery` `map.hospital` `map.repair_bay` `map.repair_pad` `map.arty_plat` `map.reinforce` + enemy buildings.

---

## Slice-1 tree

Aegis only: `prod.command`, `power.fusion`, `prod.barracks`, `eco.dropoff`, `prod.factory`, `def.patriot`. Units: dozer, chinook, rifle, rocket, basic tank, scout gun. Research: `up.capture`.
