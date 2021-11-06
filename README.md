# rules_tsql
TSQL Rules for Bazel

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

## Features
1. Compile a DACPAC on any platform
2. No Visual Studio Build tools dependencies
3. Compile Dacpacs with references to other dacpacs
4. MSBuild .sqlproj feature parity 

## Usage
WORKSPACE
```python
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
http_archive(
    name = "rules_msbuild",
    sha256 = "607a251ed80ef195c85edd95689df96e7aae97911bbbf0be1884594d32d8472a",
    urls = ["https://github.com/samhowes/rules_msbuild/releases/download/0.0.10/rules_msbuild-0.0.10.tar.gz"],
)
load("@rules_msbuild//dotnet:deps.bzl", "msbuild_register_toolchains", "msbuild_rules_dependencies")
msbuild_rules_dependencies()
msbuild_register_toolchains(version = "host")

http_archive(
    name = "rules_tsql",
    sha256 = "510e21cf063e3dbc906509656e5a6dabfc32be1916384cb1208492ffdd603957",
    urls = ["https://github.com/samhowes/rules_tsql/releases/download/0.0.1/rules_tsql-0.0.1.tar.gz"],
)
load("@rules_tsql//tsql:deps.bzl", "rules_tsql_dependencies")
load("@rules_tsql//tsql:defs.bzl", "tsql_register_toolchains")
rules_tsql_dependencies()
tsql_register_toolchains()
```

NuGet Packages:
```python
load("@rules_tsql//deps:public.nuget.bzl", "TSQL_FRAMEWORKS", "TSQL_PACKAGES")
load("@rules_msbuild//deps:public_nuget.bzl", "FRAMEWORKS", "PACKAGES")
load("@rules_msbuild//dotnet:defs.bzl", "nuget_deps_helper", "nuget_fetch")

nuget_fetch(
    name = "nuget",
    packages = {},
    deps = nuget_deps_helper(FRAMEWORKS, PACKAGES) +
           nuget_deps_helper(TSQL_FRAMEWORKS, TSQL_PACKAGES), 
)
```

See `//tests/examples` for more usage examples

## Details
Compilation is done using [SqlBuildTask](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql.sqlbuildtask?view=sql-datatools-msbuild-16) from [Microsoft.Data.Tools.Schema.Tasks.Sql.dll](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql?view=sql-datatools-msbuild-16) available via the [DacFx package](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx/150.5290.2-preview) (preview versions have the Tasks dll).
