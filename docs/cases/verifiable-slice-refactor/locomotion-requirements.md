# Locomotion requirements (before asset work)

Authorized default (Q3): **no new art direction**; infantry/vehicle **root motion + facing**; chinook **rotor spin** on existing `rotor_*` assemblies; **structures static**. No skeletal walk-cycle art. Human visual/play acceptance remains the user at close.

Preserve the current readable toy-soldier / hard-surface look already in `assets/models/` (team_color materials, scale, upright +Y / forward −Z).

## Goals

1. Moving ground units read as moving (not sliding ice with zero motion cues).
2. Facing reads order intent (move heading or aim when attacking).
3. Chinook reads as aircraft via rotor motion.
4. Buildings/defenses/docks/garrisons stay planted — no bob, no fake “idle breathe” required.

## Non-goals

- Armatures, skinning, or authored walk/run cycles that imply a skeleton.
- New silhouette, proportion, or material direction.
- Universal frame-perfect animation sync across GPUs.
- Locomotion for roles not in the slice roster.

## Role policy

| Kind | Roles (slice) | Motion |
| --- | --- | --- |
| Infantry | `inf.rifle`, `inf.rocket` | Root translation toward sim pose; optional tiny vertical bob (code or allowlisted clip). Yaw to move or aim. |
| Ground vehicles | `build.dozer`, `armor.basic`, `veh.scout_gun` | Root translation; yaw to velocity/aim. Optional subtle wheel/body lean **without** new meshes if not already authored. |
| Air gatherer | `eco.chinook` (`gatherer.glb`) | Root translation + altitude already used by client; **must** spin `rotor_left` and `rotor_right` while alive/operating. |
| Structures / map | `prod.*`, `power.fusion`, `eco.dropoff`, `def.patriot`, `map.dock`, `map.garrison`, clutter `rocks` | Static root pose; no locomotion animation. |

## Technical constraints

- Pipeline remains Blender → glTF (`tools/art/build_assets.py`) → Godot import. Sim stays authoritative; presentation is cosmetic.
- Assets today have **no armatures**. Keep that invariant for walk cycles.
- Manifest already lists chinook assemblies `body`, `rotor_left`, `rotor_right`. Client may spin those nodes in code even if glTF animations stay empty.
- `verify_assets.py` policy after C freeze: **reject** skeletal/armature walk cycles; **allow** only the allowlist below (including “no glTF animations + code-driven rotors/root bob”).
- Instant combat tracers are a separate claim (`combat.instant_tracer_tied_to_shot`); locomotion must not depend on them.

## Allowlist (animation policy)

Allowed:

- Zero glTF animations, with client-driven root bob + rotor spin.
- Optional glTF **node** animations that only rotate `rotor_left` / `rotor_right` on `gatherer`.
- Optional simple root-channel bob on infantry/vehicle glTFs **without** introducing armature skins.

Forbidden:

- Humanoid/vehicle skeletal walk or run cycles.
- New bones for locomotion.
- Animations on structures/map props for “life.”

## Acceptance (engineering vs user)

**Engineering (in case proof):**

- Moving infantry/vehicle: root position advances with sim; yaw changes when direction changes within a short presentation window.
- Chinook: both rotor assemblies rotate while entity alive and not contained; stop or freeze when dead/removed.
- Structures: no locomotion motion channels active.
- `python3 tools/art/verify_assets.py` passes under the frozen allowlist.
- Matrix rows `presentation.locomotion_root_motion`, `presentation.chinook_rotor_spin`, `assets.no_skeletal_walk`.

**User (close gate):**

- Visual/play feel of locomotion and rotors. Engineering supplies a short capture/package; user accepts or overrules.

## Implementation split

- **C:** freeze verify_assets allowlist text/asserts.
- **I-client:** root motion/facing + rotor spin in `src/Client/Battlefield.cs` (and related).
- **I-assets:** only if glTF clips or mesh assembly fixes are needed; otherwise keep GLBs animation-free and satisfy policy via allowlist + client motion.
