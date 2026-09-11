**BLOCK**

- **P2 — Guard cannot follow/protect a target.** [src/Client/CommandIntent.cs:12](/Users/oezguercelebi/GitHub/iron-doctrine/src/Client/CommandIntent.cs:12) discards Guard’s picked entity ID. Selecting infantry, pressing G, and clicking a moving friendly Dozer therefore submits a point guard; the infantry cannot follow it. This breaks the accepted `MECHANICS.md:28` behavior despite simulation support for targeted Guard. The targeted client proof asserts this incorrect target stripping.

Proof gap: manual GUI evidence covers earlier commit `bb5c572` and omits Guard and specialized interactions; final pilot victory does not exercise those input paths.

Inspected the final screenshot: Victory at 02:15 with Rematch/Quit visible. Supplied final build, import, headless, and simulation logs report success; no tests were run during this review.