# Factions and commanders

Source doctrines. Our working names in the last section. Implementable factions: [docs/catalog/FACTIONS.md](../catalog/FACTIONS.md).

## How to read a side

Each side is a **logistics story** plus a **combat story**. If those two stories match, the faction is honest.

---

## USA — quality, air, intel

**Logistics.** Slowest economy for the cost of the army. Chinooks pull $600 a trip and fly, so a dock across a mountain is still yours — until a quad cannon exists. Power is tight. Units are expensive.

**Combat.** Best air force in the game (Raptor, stealth fighter, Aurora, Comanche, Chinook as transport). Ground vehicles can hang a **battle drone** (repair) or **spy drone** (vision). Rangers flashbang and combat-drop into garrisons. Pathfinders delete infantry. Paladin lasers eat missiles. Particle Cannon is the only superweapon you can walk next to.

**Hole.** No mass. Lose Chinooks and the money stops. Lose AA-free sky and Granger (ZH) laughs. Early game is "get the base up before someone walks in."

**Hero:** Colonel Burton.

**Tech building:** Strategy Center — upgrades plus a global **battle plan**.

**Vanilla powers (skirmish, Generals):**

| Rank | Spend |
| --- | --- |
| 1-star | Paladin unlock, Stealth Fighter unlock, Spy Drone |
| 3-star | Pathfinder unlock, Para-drop 1–3, A-10 1–3, Emergency Repair 1–3 |
| 5-star | Fuel Air Bomb |

Zero Hour adds Leaflet Drop, Spectre Gunship, MOAB-class upgrade on the bomb, and more.

### Zero Hour USA generals

| General | Doctrine | Gets | Loses / pays |
| --- | --- | --- | --- |
| **Malcolm "Ace" Granger** | Air | King Raptor, Combat Chinook, stealth Comanche, carpet bomb, cheaper / stronger air | Weak ground; lives and dies on airfields |
| **"Pinpoint" Townes** | Laser | Laser tank, laser turret, cheaper Avenger, better fusion | No Paladin, no Tomahawk, no Crusader |
| **Alexis Alexander** | Superweapon | Cheap Particle Cannon, Aurora Alpha, EMP Patriots, extra fusion from rods | No Crusader / Paladin; vehicles cost more; plays defense then beam |

---

## China — mass, armor, fire, nukes

**Logistics.** Cheap units, extra gatherer trucks, later hackers as a money farm. Power from nuclear reactors that punish you if they pop. Command center has **no radar until upgraded**.

**Combat.** Horde bonus: five-plus Red Guard, Tank Hunters, or Battlemasters shoot faster (Nationalism upgrade makes it bigger). Overlord is the mammoth: bunker, gatling, or speaker-tower add-on. Napalm (Dragon, Inferno, MiG) makes firestorms. Nuke Cannon is a point-buy artillery nuke. EMP and the Nuclear Missile are the late hammer. Black Lotus and Hackers are the electronic-warfare kit. Speaker towers heal.

**Hole.** Slow ground. Thin air (MiGs, later Helix). You win a long fight and lose a raid.

**Hero:** Black Lotus.

**ZH buildings that matter:** Internet Center (hackers as a structure), ECM tank, Listening Outpost, Helix.

**Vanilla powers (skirmish, Generals):**

| Rank | Spend |
| --- | --- |
| 1-star | Red Guard Training, Artillery Training, Nuke Cannon unlock |
| 3-star | Cluster Mines, Artillery Barrage 1–3, Cash Hack 1–3, Emergency Repair 1–3 |
| 5-star | EMP Pulse |

Zero Hour adds Carpet Bomb and Frenzy.

### Zero Hour China generals

| General | Doctrine | Gets | Loses / pays |
| --- | --- | --- | --- |
| **Ta Hun Kwai** | Tanks | Cheaper / stronger tanks, Emperor Overlord | No Inferno, no Nuke Cannon; air costs more |
| **"Anvil" Shin Fai** | Infantry | Mini-gunners, infantry-focused powers and bunkers | Weak armor line |
| **Tsing Shi Tao** | Nuclear | Nuke shells and nuke MiGs from the start of his tree, nuke carpet bomb | Plays dirty; accidents are the joke in the fiction |

---

## GLA — cheap, stealth, salvage, no grid

**Logistics.** Workers build *and* harvest. No power plants. Black Market trickles cash and sells upgrades. Salvage crates upgrade Technicals, Quads, Marauders in the field. Cash bounty power pays you for killing. Fake buildings (ZH) waste the other player's time.

**Combat.** Hit-and-run, not a mirror tank fight. Tunnel network is a map hack. Demo traps, disguised bomb trucks, terrorists in civilian cars, hijackers, Jarmen Kell shooting drivers. Quad Cannon is the AA that makes USA cry. Scud Storm is the superweapon. **GLA hole:** unless you crush the remnant, the building comes back.

**Hole.** No air force. Thin armor unless scavenged and upgraded. Direct even fight against Overlords or Paladins is a loss. You need numbers, angles, or theft.

**Hero:** Jarmen Kell.

**Vanilla powers (skirmish, Generals):**

| Rank | Spend |
| --- | --- |
| 1-star | Scud unlock, Marauder unlock, Technical Training |
| 3-star | Hijacker unlock, Rebel Ambush 1–3, Cash Bounty 1–3, Emergency Repair 1–3 |
| 5-star | Anthrax Bomb |

Zero Hour adds GPS Scrambler and Sneak Attack (drop a tunnel entrance).

### Zero Hour GLA generals

| General | Doctrine | Gets | Loses / pays |
| --- | --- | --- | --- |
| **Dr. Thrax** | Toxin | Toxin rebels, toxin tunnels, anthrax already on, stronger Scud Storm toxin | No demo/high-explosive kit; no Hijacker / Saboteur; no camouflage line |
| **Rodall "Demo" Juhziz** | Explosives | Stronger bombs, demo on units, traps | Less toxin / stealth |
| **Prince Kassad** | Stealth | Camo rebels from minute one, cloaked Hijacker, GPS Scrambler earlier | No tanks, no Scuds — if you are seen, you are dead |

---

## Challenge boss

**General Leang** (AI-only in Challenge): mixed tech from all three sides. Not a skirmish loadout we copy 1:1. Useful as "the AI that cheats by being a fourth doctrine."

Cut / unused generals from development (Bradley, Griffon, Thorn, Ironside as a playable, etc.) are trivia, not a requirement.

---

## Shared skeleton (so our factions can rhyme)

Every side in the source has:

- Builder
- Supply drop-off
- Barracks, vehicle plant, (air or a substitute)
- Tech structure
- Superweapon
- One hero
- Capture-capable basic infantry
- A rocket infantry
- A basic tank (GLA's is weaker until upgraded)
- A promotion tree with more options than points

That skeleton is what Iron Doctrine should keep. The personality is what happens when you **break** the skeleton (no air, no power, no tanks, only air, only nukes).

---

## Iron Doctrine working map

| Source | Our working name | One-line doctrine |
| --- | --- | --- |
| USA vanilla | **Aegis** | Precision, air, expensive quality, intel |
| China vanilla | **Forge** | Numbers, armor, industry, lingering area denial |
| GLA vanilla | **Veil** | No grid, stealth, salvage, tunnels, cheap swarm |
| ZH specialists | Named commanders under those three | Same 3×3 grid: air / laser / superweapon; tank / infantry / nuke; toxin / demo / stealth — **renamed** |

Specialist names are not chosen. Do not use Granger, Townes, Alexander, Kwai, Shin Fai, Tao, Thrax, Juhziz, Kassad.

Veil copies GLA **rules** (holes, workers, salvage, no power). It does not copy GLA **fiction**.
