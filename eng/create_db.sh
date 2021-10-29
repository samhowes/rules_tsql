#!/usr/bin/env bash
set -euo pipefail

docker pull mcr.microsoft.com/mssql/server
docker run mcr.microsoft.com/mssql/server
