#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
echo "HELPER_NOT_GUI src/Client/Proof/run.sh — CommandIntent/SelectionIntel only; no Godot mouse, HUD, or rendering"
DOTNET_BIN="${IRON_DOTNET:-$ROOT/.tools/dotnet/dotnet}"
if [ ! -x "$DOTNET_BIN" ]; then DOTNET_BIN="$(command -v dotnet)"; fi
PROOF_DIR="$(mktemp -d "${TMPDIR:-/tmp}/iron-client-input-proof.XXXXXX")"
trap 'rm -rf "$PROOF_DIR"' EXIT
cat > "$PROOF_DIR/ClientInputProof.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <DefineConstants>CLIENT_INPUT_PROOF</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$ROOT/src/Contracts/*.cs" />
    <Compile Include="$ROOT/src/Sim/*.cs" />
    <Compile Include="$ROOT/src/Client/CommandIntent.cs" />
    <Compile Include="$ROOT/src/Client/SelectionIntel.cs" />
    <Compile Include="$ROOT/src/Client/Proof/ClientInputProof.cs" />
  </ItemGroup>
</Project>
EOF
"$DOTNET_BIN" run --project "$PROOF_DIR/ClientInputProof.csproj" -- "$ROOT/data/slice1.placeholders.json"
