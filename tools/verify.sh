#!/usr/bin/env bash
# Fail-closed verify entry. Missing/skipped/timeout is never pass. Does not use dotnet test.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
if [ -x "$ROOT/.tools/dotnet/dotnet" ]; then
  export DOTNET_ROOT="$ROOT/.tools/dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

OUTDIR="${VERIFY_OUTDIR:-$ROOT/artifacts/verify}"
GROUP=""
SCENARIO=""
LIST=0
GATE=0
JSON_ONLY=0

usage() {
  cat <<'EOF'
tools/verify.sh — named scenarios/groups, structured JSON, fail-closed.

  (no args)              required integrated suite (sim, sealed replay, acceptance, assets,
                         client-intent, review-harness, client-gui; audit info; render pending)
  --list                 print groups and scenarios
  --scenario <id>        run one scenario
  --group <name>         sim|acceptance|client-intent|assets|audit|review-harness|client-gui|render
  --gate                 reserved; audit still does not fail on stuck counts (Q5)
  --json                 print outcomes JSON to stdout as well as artifacts/verify/outcomes.json

Default suite runs sim, sealed replay, acceptance, assets, client-intent, review-harness,
and client-gui (Godot PushInput). Audit is info. render stays pending without a display.
client-intent is helper-not-GUI. Audit is not a fail gate. Does not use dotnet test.
EOF
}

while [ $# -gt 0 ]; do
  case "$1" in
    --list) LIST=1 ;;
    --scenario)
      shift
      [ $# -gt 0 ] || { echo "missing --scenario id" >&2; exit 2; }
      SCENARIO="$1"
      ;;
    --group)
      shift
      [ $# -gt 0 ] || { echo "missing --group name" >&2; exit 2; }
      GROUP="$1"
      ;;
    --gate) GATE=1 ;;
    --json) JSON_ONLY=1 ;;
    -h|--help) usage; exit 0 ;;
    *)
      echo "unknown argument: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
  shift
done

if [ "$LIST" -eq 1 ] && { [ -n "$GROUP" ] || [ -n "$SCENARIO" ]; }; then
  echo "--list cannot be combined with --group/--scenario" >&2
  exit 2
fi
if [ -n "$GROUP" ] && [ -n "$SCENARIO" ]; then
  echo "--group and --scenario cannot be combined" >&2
  exit 2
fi

mkdir -p "$OUTDIR"

list_items() {
  python3 - "$ROOT" <<'PY'
import re, sys
from pathlib import Path
root = Path(sys.argv[1])
text = (root / "tests/ScenarioHarness.cs").read_text()
block = re.search(r"public static class ScenarioIds\s*\{(.*?)\n\}", text, re.S)
ids = re.findall(r'= "([^"]+)"', block.group(1) if block else "")
groups = [
    ("sim", "required", "SimProof default groups via tools/proof.sh; sealed replay"),
    ("acceptance", "required", "Frozen C acceptance contracts (currently red until I-sim)"),
    ("client-intent", "required", "src/Client/Proof/run.sh helper-not-GUI"),
    ("assets", "required", "python3 tools/art/verify_assets.py"),
    ("audit", "info", "tools/audit.sh report; not a fail gate (Q5)"),
    ("review-harness", "required", "tools/review_case.py --self-check"),
    ("client-gui", "required", "src/Client/Proof/verify-godot.sh PushInput through production _Input"),
    ("render", "pending", "Controlled frame capture; headless is not rendering evidence"),
]
print("GROUPS")
for name, gate, note in groups:
    print(f"  {name:16} {gate:10} {note}")
print("SCENARIOS")
seen = []
for cid in ids:
    if cid in seen:
        continue
    seen.append(cid)
    print(f"  {cid}")
for extra in (
    "verify.sealed_replay",
    "verify.entry_fail_closed",
    "verify.review_case_generalized",
):
    if extra not in seen:
        print(f"  {extra}")
PY
}

now() { python3 -c 'import time; print(time.time())'; }

write_outcome() {
  local id="$1" status="$2" duration="$3" reason="$4" artifacts="$5" gate="$6"
  python3 - "$OUTDIR" "$id" "$status" "$duration" "$reason" "$artifacts" "$gate" <<'PY'
import json, sys
from pathlib import Path
outdir, oid, status, duration, reason, artifacts, gate = sys.argv[1:8]
if status not in ("pass", "fail", "pending"):
    raise SystemExit("invalid status " + status)
safe = oid.replace("/", "_")
path = Path(outdir) / (safe + ".outcome.json")
arts = [a for a in artifacts.split("\n") if a]
obj = {
    "id": oid,
    "status": status,
    "duration": float(duration),
    "artifacts": arts,
    "reason": reason,
    "gate": gate == "1",
}
path.write_text(json.dumps(obj, indent=2) + "\n")
print(f"VERIFY {oid:42} {status:8} {float(duration):7.3f}s  {reason[:160]}")
PY
}

reason_from_log() {
  local log="$1" code="$2"
  python3 - "$log" "$code" <<'PY'
import pathlib, sys
log, code = pathlib.Path(sys.argv[1]), sys.argv[2]
text = log.read_text(errors="replace") if log.is_file() else ""
lines = [ln.rstrip() for ln in text.splitlines() if ln.strip()]
fails = [ln for ln in lines if ln.startswith("FAIL") or "FAIL " in ln[:20]]
if fails:
    print(" | ".join(fails[-4:]))
    raise SystemExit
pending = [ln for ln in lines if "PENDING" in ln]
if pending:
    print(pending[-1])
    raise SystemExit
if lines:
    print(lines[-1][:400])
    raise SystemExit
print("exit " + code)
PY
}

run_cmd() {
  local id="$1"
  local gate="$2"
  shift 2
  local log="$OUTDIR/${id}.log"
  local start end dur code status reason
  start="$(now)"
  set +e
  "$@" >"$log" 2>&1
  code=$?
  set -e
  end="$(now)"
  dur="$(python3 -c "print(round(float('$end')-float('$start'), 3))")"
  if [ "$code" -eq 0 ]; then
    status=pass
    reason="$(reason_from_log "$log" "$code")"
  else
    status=fail
    reason="$(reason_from_log "$log" "$code")"
    if grep -q '^PENDING \|PENDING ' "$log" 2>/dev/null; then
      status=pending
    fi
  fi
  write_outcome "$id" "$status" "$dur" "$reason" "$log" "$gate"
}

emit_pending() {
  local id="$1" gate="$2" reason="$3"
  write_outcome "$id" "pending" "0" "$reason" "" "$gate"
}

dotnet_sim() {
  dotnet run --project "$ROOT/tests/SimProof.csproj" -- "$@"
}

run_sim() {
  run_cmd sim 1 bash "$ROOT/tools/proof.sh"
}

run_replay() {
  run_cmd verify.sealed_replay 1 dotnet_sim --scenario verify.sealed_replay --out "$OUTDIR/replay-sealed"
}

run_acceptance() {
  local id="${1:-acceptance}"
  run_cmd "$id" 1 dotnet_sim acceptance
}

run_assets() {
  local id="${1:-assets}"
  run_cmd "$id" 1 python3 "$ROOT/tools/art/verify_assets.py"
}

run_client_intent() {
  local log="$OUTDIR/client-intent.log"
  local start end dur code
  start="$(now)"
  set +e
  bash "$ROOT/src/Client/Proof/run.sh" >"$log" 2>&1
  code=$?
  set -e
  end="$(now)"
  dur="$(python3 -c "print(round(float('$end')-float('$start'), 3))")"
  local note="helper-not-GUI; not client-input evidence"
  if [ "$code" -eq 0 ]; then
    write_outcome client-intent pass "$dur" "$note" "$log" 1
  else
    write_outcome client-intent fail "$dur" "$note | $(reason_from_log "$log" "$code")" "$log" 1
  fi
}

run_audit() {
  local log="$OUTDIR/audit.log"
  local start end dur code
  start="$(now)"
  set +e
  bash "$ROOT/tools/audit.sh" >"$log" 2>&1
  code=$?
  set -e
  end="$(now)"
  dur="$(python3 -c "print(round(float('$end')-float('$start'), 3))")"
  if [ "$code" -ne 0 ]; then
    write_outcome audit fail "$dur" "$(reason_from_log "$log" "$code")" "$log"$'\n'"artifacts/behavior-audit.txt" 1
  else
    write_outcome audit pending "$dur" "audit report only; stuck/oscillate counts are not a fail gate (Q5)" "$log"$'\n'"artifacts/behavior-audit.txt" 0
  fi
}

run_review() {
  local id="${1:-review-harness}"
  run_cmd "$id" 1 python3 "$ROOT/tools/review_case.py" --self-check
}

run_client_gui() {
  local gate="$1"
  run_cmd client-gui "$gate" bash "$ROOT/src/Client/Proof/verify-godot.sh"
}

run_render() {
  local gate="$1"
  emit_pending render "$gate" "No controlled frame-capture gate in i-verify. Headless sim is not rendering evidence."
}

run_entry_fail_closed() {
  local nested="$OUTDIR/nested-unknown"
  mkdir -p "$nested"
  local start end dur code
  start="$(now)"
  set +e
  VERIFY_OUTDIR="$nested" bash "$ROOT/tools/verify.sh" --scenario not.a.scenario >"$OUTDIR/verify.entry_fail_closed.log" 2>&1
  code=$?
  set -e
  end="$(now)"
  dur="$(python3 -c "print(round(float('$end')-float('$start'), 3))")"
  if [ "$code" -eq 0 ]; then
    write_outcome verify.entry_fail_closed fail "$dur" "unknown scenario was reported as pass" "$OUTDIR/verify.entry_fail_closed.log" 1
  else
    write_outcome verify.entry_fail_closed pass "$dur" "unknown scenario fail-closed (exit $code)" "$OUTDIR/verify.entry_fail_closed.log" 1
  fi
}

run_named_scenario() {
  local id="$1"
  case "$id" in
    sim) run_sim; run_replay ;;
    acceptance) run_acceptance ;;
    client-intent) run_client_intent ;;
    assets) run_assets ;;
    audit) run_audit ;;
    review-harness) run_review ;;
    client-gui) run_client_gui 1 ;;
    render) run_render 1 ;;
    path.ground_traverses_unbuildable|combat.instant_tracer_tied_to_shot|build.ghost_matches_arrival_occupancy)
      run_acceptance "$id"
      ;;
    assets.no_skeletal_walk) run_assets "$id" ;;
    verify.sealed_replay) run_replay ;;
    verify.review_case_generalized) run_review "$id" ;;
    verify.entry_fail_closed) run_entry_fail_closed ;;
    path.distinguish_wait_vs_stuck|path.air_ignores_ground_clutter|combat.tank_cannot_hit_chinook|combat.rocket_missile_kills_chinook|combat.missile_exists_before_impact|combat.no_duplicate_missile_impact|build.unbuildable_blocks_place)
      run_sim
      ;;
    client.no_fog_world_leak|client.input_build_ghost_point|match.pause_freezes_clock)
      run_cmd "$id" 1 bash "$ROOT/src/Client/Proof/verify-godot.sh" "$id"
      ;;
    client.*|presentation.*)
      emit_pending "$id" 1 "GUI/render scenario not wired; missing required check is not pass"
      ;;
    *)
      write_outcome "$id" fail 0 "unknown scenario $id; missing required check is not pass" "" 1
      ;;
  esac
}

run_group() {
  case "$1" in
    sim) run_sim; run_replay ;;
    acceptance) run_acceptance ;;
    client-intent) run_client_intent ;;
    assets) run_assets ;;
    audit) run_audit ;;
    review-harness) run_review ;;
    client-gui) run_client_gui 1 ;;
    render) run_render 1 ;;
    *)
      echo "unknown group: $1" >&2
      echo "known: sim acceptance client-intent assets audit review-harness client-gui render" >&2
      exit 2
      ;;
  esac
}

run_default() {
  run_sim
  run_replay
  run_acceptance
  run_assets
  run_client_intent
  run_review
  run_audit
  run_client_gui 1
  run_render 0
}

if [ "$LIST" -eq 1 ]; then
  list_items
  exit 0
fi

# Prior group leftovers must not leak into this run's fail-closed exit.
rm -f "$OUTDIR"/*.outcome.json

if [ -n "$SCENARIO" ]; then
  run_named_scenario "$SCENARIO"
elif [ -n "$GROUP" ]; then
  run_group "$GROUP"
else
  run_default
fi

python3 - "$OUTDIR" "$JSON_ONLY" <<'PY'
import json, sys
from pathlib import Path
outdir = Path(sys.argv[1])
print_json = sys.argv[2] == "1"
outcomes = []
for path in sorted(outdir.glob("*.outcome.json")):
    if path.name == "outcomes.json":
        continue
    outcomes.append(json.loads(path.read_text()))
exit_code = 0
for row in outcomes:
    status = row.get("status")
    gate = bool(row.get("gate", True))
    if status == "pass":
        continue
    if status == "fail":
        exit_code = 1
        continue
    if status == "pending":
        if gate:
            exit_code = 1
        continue
    exit_code = 1
report = {"exit": exit_code, "fail_closed": True, "outcomes": outcomes}
(outdir / "outcomes.json").write_text(json.dumps(report, indent=2) + "\n")
print(f"VERIFY_EXIT {exit_code} outcomes={len(outcomes)} -> {outdir / 'outcomes.json'}")
if print_json:
    print(json.dumps(report, indent=2))
sys.exit(exit_code)
PY
