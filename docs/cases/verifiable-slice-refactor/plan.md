# Plan — verifiable-slice-refactor

Max three I lanes concurrent. Shared file or import edge without freeze ⇒ one row. Hubs = lead.

## Contract freeze (first)

- [x] `c-freeze` done · lane `src/Contracts/MatchContract.cs` `src/Contracts/GameConfig.cs` `tests/SeamConformance.cs` `tests/AcceptanceContracts.cs` `tests/ScenarioHarness.cs` `tests/Program.cs` `tools/art/verify_assets.py` · after — · Merged `e3a01b0`. Acceptance red for unbuildable traverse + empty CombatTraces. proof.sh green.

## Implementation (after freeze)

Recommended wave 1 (≤3): `i-verify`, `i-sim`, `i-client`. Wave 2: `i-assets`. Then lead integrate.

- [x] `i-verify` done · `949094d` · R APPROVE
- [x] `i-sim` done · `68d5b72` · R APPROVE
- [x] `i-client` done · `3c8d698` · R APPROVE
- [x] `i-assets` done · `fa0c920` · R APPROVE (README glob expansion logged)

## Lead / integrate

- [x] `lead-hubs` done · AGENTS.md verify table; `tools/verify.sh` client-gui wired
- [ ] `integrate-proof` open · lane — · after i-verify, i-sim, i-client, i-assets, lead-hubs · Integrated `bash tools/verify.sh` exit 0 on `b649bd0` (render group still pending in harness; macOS frames captured separately). Negative controls recorded. Final case R next. User closes. No push.
