#!/usr/bin/env bash
# Headless Godot input-path evidence. PushInput through production MatchClient._Input.
# Does not claim a render pass. Missing VERIFY_PASS is a failure.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
cd "$ROOT"
if [ -x "$ROOT/.tools/dotnet/dotnet" ]; then
  export DOTNET_ROOT="$ROOT/.tools/dotnet"
  export PATH="$DOTNET_ROOT:$PATH"
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
GODOT_BIN="${IRON_GODOT:-$ROOT/.tools/godot/Godot_mono.app/Contents/MacOS/Godot}"
if [ ! -x "$GODOT_BIN" ]; then
  GODOT_BIN="$(command -v godot-mono || command -v godot || true)"
fi
if [ -z "${GODOT_BIN}" ] || [ ! -x "$GODOT_BIN" ]; then
  echo "Install Godot 4.7.2 .NET and set IRON_GODOT." >&2
  exit 1
fi
dotnet build IronDoctrine.csproj --nologo
"$GODOT_BIN" --headless --editor --path "$ROOT" --import --quit
fail=0
ids=(client.no_fog_world_leak client.input_build_ghost_point match.pause_freezes_clock)
if [ "$#" -gt 0 ]; then ids=("$@"); fi
for id in "${ids[@]}"; do
  echo "=== VERIFY $id ==="
  out="$(mktemp "${TMPDIR:-/tmp}/iron-verify.XXXXXX")"
  set +e
  "$GODOT_BIN" --headless --path "$ROOT" --quit-after 2400 -- --verify-scenario="$id" --verify-quit >"$out" 2>&1
  status=$?
  set -e
  cat "$out"
  if ! grep -q "VERIFY_PASS scenario=$id" "$out"; then
    echo "VERIFY_FAIL missing PASS line for $id (godot exit $status)"
    fail=1
  fi
  rm -f "$out"
done
exit "$fail"
