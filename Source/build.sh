#!/usr/bin/env bash
# Builds Assemblies/Witcher.dll without NuGet by invoking csc.dll directly and
# referencing the game's own Mono DLLs. Works fully offline.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MANAGED="${RIMWORLD_MANAGED:-/Users/tristan/Library/Application Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed}"
DOTNET="${DOTNET:-/usr/local/share/dotnet/dotnet}"
CSC_DLL="${CSC_DLL:-/usr/local/share/dotnet/sdk/6.0.422/Roslyn/bincore/csc.dll}"

if [[ ! -d "$MANAGED" ]]; then
  echo "ERROR: RimWorld Managed folder not found at: $MANAGED" >&2
  exit 1
fi
if [[ ! -f "$CSC_DLL" ]]; then
  echo "ERROR: Roslyn csc.dll not found at: $CSC_DLL" >&2
  exit 1
fi

OUT_DIR="$REPO_ROOT/Assemblies"
mkdir -p "$OUT_DIR"

REFS=(
  "$MANAGED/mscorlib.dll"
  "$MANAGED/System.dll"
  "$MANAGED/System.Core.dll"
  "$MANAGED/Assembly-CSharp.dll"
  "$MANAGED/Assembly-CSharp-firstpass.dll"
  "$MANAGED/UnityEngine.CoreModule.dll"
  "$MANAGED/UnityEngine.IMGUIModule.dll"
)

REF_ARGS=()
for r in "${REFS[@]}"; do
  if [[ ! -f "$r" ]]; then
    echo "ERROR: missing reference: $r" >&2
    exit 1
  fi
  REF_ARGS+=("-reference:$r")
done

SRC_FILES=()
while IFS= read -r f; do SRC_FILES+=("$f"); done < <(find "$REPO_ROOT/Source/Witcher" -name '*.cs' -type f | sort)
if [[ ${#SRC_FILES[@]} -eq 0 ]]; then
  echo "ERROR: no source files found" >&2
  exit 1
fi

echo "Building Witcher.dll..."
"$DOTNET" exec "$CSC_DLL" \
  -nologo \
  -nostdlib \
  -target:library \
  -langversion:latest \
  -warn:3 \
  -optimize+ \
  -deterministic \
  "${REF_ARGS[@]}" \
  -out:"$OUT_DIR/Witcher.dll" \
  "${SRC_FILES[@]}"

echo "Built: $OUT_DIR/Witcher.dll"
ls -lh "$OUT_DIR/Witcher.dll"
