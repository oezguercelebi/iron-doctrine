#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
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
if [ -z "$GODOT_BIN" ] || [ ! -x "$GODOT_BIN" ]; then
  echo 'Install Godot 4.7.2 .NET and set IRON_GODOT to its executable.' >&2
  exit 1
fi
VERSION="$("$GODOT_BIN" --version)"
case "$VERSION" in
  4.7.2.stable.mono.*|4.8.stable.mono.*|4.8.*.stable.mono.*) ;;
  *) echo "Unsupported engine: $VERSION. This case requires 4.7.2 .NET or stable 4.8 .NET." >&2; exit 1 ;;
esac
dotnet build IronDoctrine.csproj --nologo
"$GODOT_BIN" --headless --editor --path "$ROOT" --import
exec "$GODOT_BIN" --path "$ROOT" "$@"
