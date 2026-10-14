#!/usr/bin/env bash
# Packs Dagger.Analyzers and checks that clean code builds with warnings as
# errors, and that the violation sample reports the rules this package exists to enforce.
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"

dotnet pack src/Dagger.Analyzers/Dagger.Analyzers.csproj -o artifacts -c Release --nologo
# Same version is rebuilt locally; drop the cached nupkg so restore sees this pack.
# The MSBuild server keeps imported .targets in memory, so shut it down after the cache wipe.
rm -rf "${HOME}/.nuget/packages/dagger.analyzers"
rm -rf samples/Clean/bin samples/Clean/obj samples/Violations/bin samples/Violations/obj
dotnet build-server shutdown >/dev/null

echo "Building samples/Clean with warnings as errors..."
dotnet build samples/Clean/Clean.csproj -c Release -warnaserror --nologo

echo "Building samples/Violations..."
log="$(mktemp)"
dotnet build samples/Violations/Violations.csproj -c Release --nologo >"$log" 2>&1 || true
cat "$log"

required=(
  VSTHRD200
  VSTHRD100
  IDE0020
  CA1502
  RCS1208
  RS0030
  IDISP001
  CA1829
)
missing=0
for code in "${required[@]}"; do
  if ! grep -q "$code" "$log"; then
    echo "Missing diagnostic $code"
    missing=1
  fi
done

if ! grep -q "below '21'" "$log"; then
  echo "CA1502 is not using threshold 20"
  missing=1
fi

rm -f "$log"
if [[ "$missing" -ne 0 ]]; then
  exit 1
fi

echo "Dagger.Analyzers verification passed."
