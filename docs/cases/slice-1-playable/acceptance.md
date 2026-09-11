# Acceptance evidence

Status is pending until each evidence path is populated. This checklist does not replace the accepted intent.

| Requirement | Required evidence |
| --- | --- |
| Eight slot/team/loadout model; two filled | Frozen seam + simulation regression |
| Single tick, order list, independent deterministic state | Plain .NET build; replay state hashes incl pending/AI/fog state |
| Command+dozer boot, recoverable army wipe, building elimination, resign | Seam, full match, GUI result screen |
| Legal dozer building/forward base; builder death loses incomplete | Construction regression; GUI ghost/place |
| Finite docks, single loader, owner-only return, free first gatherer | Economy regression; local gather visible |
| Low power disables AA/radar, radar never grants vision | Power/intel regression and HUD |
| Production/prerequisites/research/cancel/refunds/rally | Regression and production HUD |
| Correct qualitative counters, air exclusion, missiles, crush/FF | Combat fixtures + visible missile rendering |
| Garrison entry/exit/armor/spill; small arms poor | Garrison fixtures and input coverage |
| Scout five infantry fire-out; gatherer transport without combat-drop | Transport regression and UI unload |
| Veteran/Elite/Heroic, XP, buffs, self heal | Veterancy regression and HUD |
| Interruptible capture, transfer, no free gatherer mint on capture | Capture regression |
| Dozer building repair and factory vehicle repair | Repair regression |
| Same-cash/fog/order AI base/gather/compose/attack/defend | AI regression + complete order-driven match |
| Every requested player input and control groups | Controls map + visible UI checks |
| Original mirrored map, no turret wall, close/contest docks | Map data validation and visible inspection |
| Real Blender glTF for every body, original assets | Art manifest/validation/contact sheet; Godot import |
| All gameplay placeholders in one file, no excluded content | Independent source review and scope checks |

## Integration gates for this stack

1. Plain .NET simulation regression executable.
2. Deterministic complete match proof (same inputs, same state/outcome).
3. Godot 4.7.2 .NET build without errors.
4. Original glTF validation and Godot import without asset errors.
5. Godot headless start without runtime exceptions.
6. Visible local play from boot through victory or defeat, plus rematch/pause checks.
7. Lane/scope hygiene, proof recorded on main, and fresh independent case R approval.

Crew's TypeScript example command is not applicable to the locked C# stack. No npm project will be added for that example.
