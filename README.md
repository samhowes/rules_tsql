# rules_tsql

TSQL Rules for Bazel

| Windows                                                                                                                                                                                                                                           | Mac                                                                                                                                                                                                                                       | Linux                                                                                                                                                                                                                                         |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [![Build Status](https://dev.azure.com/samhowes/rules_msbuild/_apis/build/status/samhowes.rules_tsql?branchName=main&jobName=windows)](https://dev.azure.com/samhowes/rules_msbuild/_build/latest?definitionId=8&branchName=main&jobName=windows) | [![Build Status](https://dev.azure.com/samhowes/rules_msbuild/_apis/build/status/samhowes.rules_tsql?branchName=main&jobName=mac)](https://dev.azure.com/samhowes/rules_msbuild/_build/latest?definitionId=8&branchName=main&jobName=mac) | [![Build Status](https://dev.azure.com/samhowes/rules_msbuild/_apis/build/status/samhowes.rules_tsql?branchName=main&jobName=linux)](https://dev.azure.com/samhowes/rules_msbuild/_build/latest?definitionId=8&branchName=main&jobName=linux) |

## Build Rules

```python
load("@rules_tsql//tsql:defs.bzl", "tsql_dacpac")

tsql_dacpac(
    name = "my_db",                   # builds `my_db.dacpac`
    srcs = glob(["my_db/**/*.sql"]),
    properties = {
        "ModelCollation": "1033, CI", # same effect as <ProeprtyGroup> elements in sqlproj
    },
)

tsql_dacpac(
    name = "consumer",
    srcs = glob(["consumer/**/*.sql"]),
    deps = [":my_db"],              # sql files can reference [my_db].[dbo].[table_name]
)

```

```shell script
bazel build //my_db                                 # compiles my_db.dacpac
bazel run //my_db:my_db.deploy  --server localhost  # deploys my_db.dacpac to localhost.my_db
bazel run //my_db:my_db.extract --server localhost  # extracts my_db to sql files on disk
```

## Features

1. Compile a DACPAC on any platform
2. No Visual Studio Build tools dependencies
3. Compile Dacpacs with references to other dacpacs
4. MSBuild .sqlproj feature parity

## Usage

WORKSPACE

<!-- rules_tsql:snippet start -->
```python
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
http_archive(
    name = "rules_tsql",
    sha256 = "e3adf6a43a6c83346535289d8d4b829123a5cf1fca56e82f884ee30c985c3c9a",
    url = "https://github.com/samhowes/rules_tsql/releases/download/0.0.6/rules_tsql-0.0.6.tar.gz",
)
load("@rules_tsql//tsql:deps.bzl", "rules_tsql_dependencies")
rules_tsql_dependencies()
load("@rules_tsql//tsql:defs.bzl", "tsql_register_toolchains")
tsql_register_toolchains()
```
<!-- rules_tsql:snippet end -->

See `//tests/examples` for more usage examples

## Details

Compilation is done using [SqlBuildTask](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql.sqlbuildtask?view=sql-datatools-msbuild-16) from [Microsoft.Data.Tools.Schema.Tasks.Sql.dll](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql?view=sql-datatools-msbuild-16) available via the [DacFx package](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx/150.5290.2-preview) (preview versions have the Tasks dll).
