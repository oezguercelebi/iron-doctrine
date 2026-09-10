# Source games

What shipped. Not our design.

## Command & Conquer: Generals (February 2003)

EA Pacific. SAGE engine. Windows, later Mac. Metacritic 84.

Near-future conventional war ("today's world plus one"): the United States and China fight the Global Liberation Army, a non-state force based in Central Asia. Separate continuity from Tiberium and Red Alert. First C&C RTS in full 3D.

### Modes

- Three campaigns, seven missions each. Chronological order is **China → GLA → USA**. All three are canon and run as one war.
- Skirmish vs computer.
- Multiplayer: LAN and internet (originally GameSpy; that network died in 2014). Custom games and "quick match." Up to eight players. Mac build had no lasting online.

### What is different from other C&C

| Older C&C | Generals |
| --- | --- |
| Tiberium / ore patches | Finite **supply docks** (default $30,000) plus smaller piles |
| MCV deploys a construction yard; buildings come from a queue | **Builder units** place buildings anywhere legal |
| Engineers capture instantly | Basic infantry research **capture** and channel it |
| Sides share a lot of chassis | Three **doctrines** that do not share a chassis |
| No commander meta-game | Combat XP unlocks **Generals Powers** |

Shared C&C DNA that stayed: power grid (except GLA), a superweapon with a visible timer, a commando, a "mammoth" tank (China's Overlord), fog of war, garrison, veterancy.

### Campaigns, one line each

- **China.** GLA nukes a Beijing parade. China contains the cell, floods the Three Gorges to stop an advance, and ends the Pacific-rim command with nuclear weapons at Dushanbe.
- **GLA.** Recovers by raiding UN and US logistics, puts down a splinter, and fires a biological payload from Baikonur.
- **USA.** Pushes through the Middle East and Kazakhstan, handles a rogue Chinese commander, and with Chinese support breaks the last GLA hold in Akmola.

### Reception and baggage

Won AIAS Computer Strategy Game of the Year. Banned in mainland China for the portrayal of the PLA (Tiananmen nuke, Three Gorges, Hong Kong convention center). Indexed in Germany; a censored "Generäle" build turned infantry into cyborgs and stripped terrorism copy. Later writing (including a 2024 RPS piece) still calls it the best-playing C&C and criticizes the GLA caricature.

### Sequel path

*Generals 2* was announced, turned into a free-to-play *Command & Conquer*, cancelled in 2013. No successor shipped.

## Command & Conquer: Generals – Zero Hour (September 2003)

Expansion. EA Los Angeles. Metacritic 83.

Same engine, same three sides. Adds units, buildings, upgrades, powers, maps, and the mode that is the real design jump: **you pick a named general**.

### Campaigns

Five missions per side. Order is **USA → GLA → China**. Live-action news briefings return.

- **USA.** Stops a Baikonur toxin shot, tracks Dr. Thrax, and captures his missiles before they fire at US cities.
- **GLA.** After Thrax, loyalists steal Kassad's stealth tech, sink a US carrier with a captured Particle Cannon, raid the US west coast, and take Stuttgart — GLA control of Europe.
- **China.** Embarrassed that GLA used Chinese hardware, China "liberates" Europe, holds the homeland, and after Hamburg is the remaining superpower. The US turns isolationist.

### The general system

Vanilla Generals already had a promotion tree. Zero Hour makes the *loadout* the faction:

- 3 vanilla sides (with the new ZH toys)
- 3 specialist generals per side
- **12 playable loadouts** in skirmish / multiplayer

Each specialist is a buff/nerf package: unique unit or building, cheaper or stronger line in one role, missing pieces in another. See [FACTIONS.md](FACTIONS.md).

### Generals' Challenge

You pick one of the nine specialists and fight a sequence of AI generals on their home maps. The enemy base is already standing. Win condition is destroy all enemy buildings. Infantry General and Demolition General are skipped as AI opponents in the default ladder. The final boss (General Leang) uses tools from all three sides.

This is the mode to steal later: named AI, pre-built base, no campaign scripting, still a puzzle.

### Zero Hour toys (roles, not a unit bible)

New roles that changed the match, not a full roster (that list is the next research pass):

- USA: garrisonable firebase, microwave (clear garrison / shut buildings down), avenger (laser AA + ground buff), sentry drone.
- China: Helix (flying Overlord-class support), ECM tank, listening outpost, Internet Center (hackers at a building).
- GLA: combat cycle (infantry-on-a-bike), battle bus, saboteur, **fake buildings** that can be upgraded into real ones.

New powers worth naming as *systems*: Spectre gunship, leaflet drop, MOAB upgrade on the fuel-air bomb, carpet bomb, frenzy, GPS scrambler, sneak-attack tunnel.

### Multiplayer

Same as Generals: LAN and internet, custom games up to eight. After GameSpy, the community used VPN tools and later C&C:Online.

## What we take

The **systems**, not the IP.

1. Three doctrines that are not the same game.
2. Builder-placed bases, not a construction yard.
3. Finite supply + secondary income.
4. Promotion bar that buys powers and unlocks.
5. Named commanders as loadouts (Zero Hour).
6. Superweapon as a visible late-game clock.
7. Skirmish vs AI first; peer multiplayer later.

## What we leave

- The 2003 campaigns and real-world place names as our story.
- EA names, models, maps, audio.
- The GLA as a terrorist joke.
- Any requirement to own or load original game files (unlike OpenSAGE).

## Related work (not this project)

- **OpenSAGE** — C# reimplementation of SAGE. Needs original data. Not playable as a game.
- **EA 2025 GPL drop** — *Generals* and *Zero Hour* source, GPLv3, no trademark grant. Preservation, not a license to ship "Generals."
- Community patches, GenLauncher, C&C:Online — keep the original executable alive.

Iron Doctrine is a new game that copies the *feel* of those systems.
