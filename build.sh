#!/usr/bin/env bash
set -euo pipefail

configuration="Release"
framework="net9.0"
runtime_linux="linux-x64"
runtime_win="win-x64"
output_dir="output"

publish() {
  local runtime="$1"
  dotnet publish -c "$configuration" -r "$runtime" -o "$output_dir/$runtime"
}

publish "$runtime_linux"
publish "$runtime_win"

echo "Done."
echo "Linux: $output_dir/$runtime_linux/dot-convert"
echo "Win  : $output_dir/$runtime_win/dot-convert.exe"
