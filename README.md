# rules_tsql
TSQL Rules for Bazel

## Coming soon...
```python
load("@rules_tsql//tsql:defs.bzl", "tsql_dacpac")

tsql_dacpac(
    name = "my_db",                   # builds `my_db.dacpac`
    srcs = glob(["my_db/**/*.sql"]),
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

## Details
Compilation is done using [SqlBuildTask](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql.sqlbuildtask?view=sql-datatools-msbuild-16) from [Microsoft.Data.Tools.Schema.Tasks.Sql.dll](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql?view=sql-datatools-msbuild-16) available via the [DacFx package](https://www.nuget.org/packages/Microsoft.SqlServer.DacFx/150.5290.2-preview) (preview versions have the Tasks dll).
