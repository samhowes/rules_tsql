#!/usr/bin/env bash
set -euo pipefail

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password1234!" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest

