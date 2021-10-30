# rules_tsql
TSQL Rules for Bazel

## Coming soon...
```python
load("@rules_tsql//tsql:defs.bzl", "tsql_dacpac")

tsql_dacpac(
    name = "my_db",             # builds `my_dby.dacpac` with DACFX
    srcs = glob(["**/*.sql"]),
)
```

## Features
1. Compile a DACPAC using [DacFx](https://github.com/microsoft/DacFx) on any platform

## Todo
1. Database references 