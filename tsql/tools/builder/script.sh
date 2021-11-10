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
function required_rlocation() {
    p="$1"
    rp="$(rlocation "$p")"
    if [[ -z "$rp" ]]; then
      echo 2> "failed to find required runfiles item: $p"
      exit 1
    fi;
    echo "$rp"
}

builder="$(required_rlocation "@@builder@@")"

execute="$builder"
if [[ -n "$RULES_TSQL_TESTING" ]]; then
  execute="echo $builder"
fi;
args=@@args@@
runfiles_args=@@runfiles_args@@

for i in "${!runfiles_args[@]}"; do
  if [[ "${runfiles_args[$i]}" == "--"* ]]; then continue; fi
  runfiles_args[$i]="$(required_rlocation "${runfiles_args[$i]}")"
done

cd "$BUILD_WORKSPACE_DIRECTORY"
$execute "${args[@]}" "${runfiles_args[@]}"
