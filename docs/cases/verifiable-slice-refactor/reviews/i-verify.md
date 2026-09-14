# R · i-verify

Reviewed: `24b3e5c..949094d`
Harness: `pr-review-toolkit:code-reviewer` · grok-4.6 · xhigh · no write

VERDICT: APPROVE
Reviewed revision: 949094d

Fail-closed verify entry, sealed replay I/O, parameterized review_case. Missing proof / BLOCK / exit-0-without-APPROVE do not pass.

## Gaps / nits (logged, not patched)

- Default `tools/verify.sh` ungates client-gui/render (`gate=0`). Explicit `--group` stays gated. Integrate must not treat default exit 0 as GUI/render proof.
- Named acceptance ids run the whole C suite.
- Replay probe is 48-tick same-process, not a finished-match fresh-process twin (i-sim covers match replay).
- `--case` path not exist-checked.
