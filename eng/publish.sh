#!/bin/bash

pkg="$1"
database_name="$(basename "$pkg")"
dacpac="bazel-bin/$pkg/$database_name.dacpac"

sqlpackage /Action:publish \
  /TargetTimeout:5 \
  /TargetServerName:localhost \
  /TargetUser:sa \
  /TargetPassword:Password1234! \
  /TargetDatabaseName:"$database_name" \
  /SourceFile:"$dacpac" \
  /p:CreateNewDatabase=true
