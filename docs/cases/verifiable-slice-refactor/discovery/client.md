# Discovery: client / input / presentation / assets

Explore `01a0a1dc-dff1-70a2-8500-6113861721c5` · grok-4.5 · effort high · read-only.

## Architecture

Godot presents; `IMatch` is authority. Production path: `_Input` → HUD intercept or world pick → `CommandIntent.Create` → `_match.Submit`.

## Proven vs not

- `src/Client/Proof/run.sh` is plain C# (`CommandIntent` + `SelectionIntel` + sim). No Godot, mouse, HUD, render.
- `ProofPilot` still bypasses GUI (`ProofPilot.Send` → `ReceiveProofOrder` → `_match.Submit`).
- Manual GUI subset exists in closed case `proof/manual-input.md`; combat/capture/transport/repair not claimed as GUI-executed there.
- No independent screen-coordinate oracle; box-select uses production `ScreenPosition`.

## Assets / locomotion

- `verify_assets.py:34` and `build_assets.py:761` forbid glTF animations. Still true.
- Client lerps root and yaws static meshes. Chinook rotors exist in manifest (`rotor_left`/`rotor_right`) but Battlefield does not spin them.
- No armatures. Simplest maintainable locomotion: root motion/bob for infantry/vehicles; spin chinook rotors; structures stay static. Do not invent skeletal walk cycles.

## Historical leads

| Lead | Verdict |
|---|---|
| No animations; static models | **Still true** |
| Pilot bypasses GUI | **Still true** |
| Instant tracers from Activity+cooldown | **Still true**; missiles from `snapshot.Projectiles` |

## Missing control surface

Load named scenario, inject recorded InputEvents through production `_Input`, single-step ticks, inspect sim vs scene, mid-match frame capture. Not a second app. Not a public remote service.

## Seams

`MatchContract.cs`, `GameConfig.cs` + placeholders, `CommandIntent.cs`, `SelectionIntel.cs`, `Intel.cs` redaction, `assets/models/manifest.json`, `vo.*`/`fx.*`/`sfx.*` event ids.
