#!/bin/bash

# --- begin runfiles.bash initialization v2 ---
# Copy-pasted from the Bazel Bash runfiles library v2.
set -uo pipefail; f=bazel_tools/tools/bash/runfiles/runfiles.bash
source "${RUNFILES_DIR:-/dev/null}/$f" 2>/dev/null || \
 source "$(grep -sm1 "^$f " "${RUNFILES_MANIFEST_FILE:-/dev/null}" | cut -f2- -d' ')" 2>/dev/null || \
 source "$0.runfiles/$f" 2>/dev/null || \
 source "$(grep -sm1 "^$f " "$0.runfiles_manifest" | cut -f2- -d' ')" 2>/dev/null || \
 source "$(grep -sm1 "^$f " "$0.exe.runfiles_manifest" | cut -f2- -d' ')" 2>/dev/null || \
 { echo>&2 "ERROR: cannot find $f"; exit 1; }; f=; set -e
# --- end runfiles.bash initialization v2 ---

set -eo pipefail
set +u
suffix=""
if [[ "$(uname -s)" == *"NT"* ]]; then suffix=".exe"; fi;
rpath="$1$suffix"
builder="$(rlocation "$rpath")"
if [[ -z "$builder" ]]; then
  echo "failed to find builder in runfiles at: $rpath"
  exit 1
fi;

execute="$builder"
if [[ -n "$RULES_TSQL_TESTING" ]]; then
  execute="echo $builder"
fi;

cd "$BUILD_WORKSPACE_DIRECTORY"
$execute "${@:2}"
