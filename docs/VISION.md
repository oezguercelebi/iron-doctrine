# Vision

**Iron Doctrine** is a modern-warfare RTS where you play a named commander with a personal doctrine, not a generic faction slot.

The feeling we want is *Generals / Zero Hour*: three sides that do not play the same, a base you plant on the map, a finite supply fight, combat that promotes you into battlefield powers, and a match that can flip on a well-timed strike.

## Goal

**Multiplayer.** Same match as Generals custom games: humans on one map, up to 8, teams or FFA, no dedicated server, no accounts. Peer-to-peer / LAN.

**First step:** one human vs one computer. Same rules, same sim. No netcode until that match is fun.

We are not shipping a single-player game that might grow a lobby. We are building the match first, locally, so two machines can later run the same match.

Also true, not the first step:

- Asymmetric factions, not reskins.
- Named commanders with strengths, holes, and unique toys (the Zero Hour idea).
- Original world. Original names. Original assets.

We are not rebuilding the 2003 campaigns, the SAGE engine, or EA's unit roster.

## What "plays like" means

A match should have these beats, in this order of importance:

1. **Plant a base.** A builder unit drops structures anywhere legal on the map. There is no MCV-style "unpack the whole base here."
2. **Mine a finite pile.** The main cash is a supply dock. It runs out. You expand, steal, or switch to secondary income.
3. **Counters matter.** Infantry, armor, air, and garrisoned buildings beat each other in a readable loop. A blob of one unit type dies to its answer.
4. **Units get better if they live.** Veterancy is visible and worth protecting.
5. **You get promoted.** Killing things fills a commander bar. You spend points on strikes, unlocks, and upgrades. That bar is the personality of the match.
6. **The sides feel different in the first two minutes.** Aegis spends and flies. Forge floods and grinds. Veil cheaps out, hides, and scavenges. If you can swap the labels and not notice, we failed.
7. **A late superweapon exists** and is a clock the other player can see and race.

## First playable

1 human vs 1 computer.

- One 2-slot map (`maps.two_flats`).
- One commander (vanilla Aegis).
- Economy, build, move, shoot, win by destroying enemy buildings.
- No netcode. No 8-slot lobby. No other factions.

The sim is still a *match* (slots, orders, clocks), not a special “AI mode.” Multiplayer reuses it.

## After that (goal path)

1. Two humans, same rules, peer-to-peer / LAN. No backend.
2. More slots and maps (4 / 6 / 8, teams, FFA).
3. Forge, Veil, specialist commanders, Challenge.

Factions and the full map roster stay in the catalog so we do not design a 1v1-only engine. We do not *build* them before 1 vs computer works, and we do not finish all 12 loadouts before the first networked match.

## Out of scope

- A backend, accounts, cloud saves, ranked ladder service, or matchmaking server.
- Using EA names, maps, models, audio, or data files.
- Forking the 2025 GPL source drop of *Generals / Zero Hour* as this product. That code is a study reference at most. This game is original.
- A story campaign in the first several slices.
- Balance numbers and product names until a later pass. Role catalog lives in `docs/catalog/`. Agent entry: `AGENTS.md` and `docs/catalog/SLICE.md`.

## Tone

Near-future conventional war. Three professional (or irregular) forces with distinct logistics, not cartoon nations and not a 2003 news-parody of terrorism. Veil is guerrilla doctrine: stealth, salvage, tunnels, no power grid. It is not a stereotype pack.

## Name

**Iron Doctrine.**

A doctrine is what a Zero Hour general *is*: a locked-in way of fighting, with a hole on the other side. Iron is the industrial, unglamorous part of an RTS — supply, armor, the clock.

Working title. Change it if a better one shows up before first public build.
