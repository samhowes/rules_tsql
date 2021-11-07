#!/bin/bash

set -euo pipefail

prebuilt="$(dirname "$0")/prebuilt"

"$DOTNET_RUNTIME_BIN" "$prebuilt/builder.dll" "${@:1}"
