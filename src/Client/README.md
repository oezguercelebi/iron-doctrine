# Slice-1 client

`MatchClient.Initialize(IMatch, Func<IMatch>)` must run before adding the node to the scene tree. The root scene attaches the lead-owned `src/Bootstrap/Main.cs`. This lane imports only `IronDoctrine.Contracts` and Godot.

The client takes one detached snapshot after **every** fixed simulation step and consumes its current-tick events before advancing again. Selection, camera, control groups, interpolation, UI and sound remain presentation state. The battlefield loads each role's configured glTF resource directly, replaces its `team_color` material per owner, and respects snapshot visibility. Radar does not reveal terrain or hidden units.

Enemy facing follows successive visible positions. Enemy combat feedback uses visible health changes and public activity transitions; it never reads redacted enemy destinations or targets or invents shot endpoints. Private order targets are used only for the viewer's own instant-fire tracers. A disabled radar suppresses live minimap entity blips, including selected units.

## Controls

| Input | Action |
| --- | --- |
| Left click / drag | Select / box-select own units |
| Shift + left click | Add/remove selection |
| Right click | Contextual move, attack, repair, gather, enter or researched capture |
| Shift + order | Append to the unit's order queue |
| Ctrl + 1–9; 1–9 | Assign / recall a control group; double-tap to focus |
| WASD / arrows | Pan |
| Middle drag / wheel | Pan / zoom |
| Space / Tab | Focus selection / find Dozer |
| Q / X / G | Attack-move / stop / guard |
| Z | Ordinary attack against a visible enemy, including after capture research |
| T / F | Append waypoint / force attack (including friendly targets) |
| R / E / C | Repair / enter / capture |
| V / Y / Delete | Exit passengers / rally / sell |
| Build card, then left click | Place a footprint checked with `IMatch.CanPlace` |
| Producer card | Queue its configured unit or research |
| Queue number × | Cancel that queue position with the configured refund |
| Minimap left / right click | Jump camera / issue contextual movement |
| H / Escape | Field guide / cancel targeting, then local pause |

The field guide pauses a running local match. Pause offers resume, guide, resign and desktop quit. The result screen offers rematch and quit. All labels for roles, costs, prerequisites, progress and capacities come from the frozen configuration/snapshot. Queue buttons retain the displayed producer's entity ID; snapshot ordering and a changed selection cannot redirect cancellation to a different producer. Enemy garrisons receive ordinary contextual Attack and cannot be selected as capture targets.

## Opt-in visible proof

After the Godot argument separator, use `--proof-play` to drive ordinary **player** orders from the player's fog-filtered snapshot, public map/setup and `CanPlace`. It does not inspect the simulation implementation or state hash, change resources, or advance one side separately. Its build and composition policy uses the configured AI policy values as a repeatable input recipe.

- `--proof-speed=4` optionally runs the shared fixed match clock four times faster; default is normal time. It does not change tick size or either side's order timings.
- `--proof-output=/absolute/path.png` saves the actual finished viewport; default `user://proof-play.png`.
- `--proof-quit` closes after the result has rendered and the image/marker have been emitted.

The start marker is `PROOF_PLAY_STARTED`. A finished game emits `PROOF_PLAY_FINISHED tick=… result=victory|defeat orders=… models=…`. The screenshot path and save result are printed separately as `PROOF_SCREENSHOT`. Headless mode emits the outcome but cannot produce a rendered screenshot.

`CLIENT_READY` reports the viewer slot, snapshot count and successfully loaded model resources. A missing asset produces a warning and a selectable footprint while import is pending; final builds must have all original glTF resources imported.
