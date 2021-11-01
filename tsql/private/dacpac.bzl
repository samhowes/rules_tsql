load(":providers.bzl", "DacpacInfo")

def tsql_dacpac_macro(name, **kwargs):
    tsql_dacpac(
        name = name,
        **kwargs
    )

    tsql_unpack(
        name = name + ".unpack",
        dacpac = ":" + name,
    )

def _dacpac_impl(ctx):
    dacpac = ctx.actions.declare_file(ctx.attr.name + ".dacpac")

    args = ctx.actions.args()
    args.add_all([
        "build",
        "--label=" + str(ctx.label),
        "--output=" + dacpac.path,
    ] + ctx.files.srcs)
    ctx.actions.run(
        mnemonic = "CompileDacpac",
        inputs = ctx.files.srcs,
        outputs = [dacpac],
        executable = ctx.executable._builder,
        arguments = [args],
    )
    return [
        DefaultInfo(
            files = depset([dacpac]),
            runfiles = ctx.runfiles(
                files = [dacpac],
            ),
        ),
        DacpacInfo(dacpac = dacpac),
    ]

def _unpack_impl(ctx):
    dacpac = ctx.attr.dacpac[DacpacInfo].dacpac
    model_xml = ctx.actions.declare_file(
        dacpac.basename.rsplit(".", 1)[0] + ".model.xml",
    )

    args = ctx.actions.args()
    args.add_all([
        "unpack",
        "--output=" + model_xml.path,
        "--dacpac=" + dacpac.path,
    ])

    ctx.actions.run(
        mnemonic = "ExtractDacpac",
        inputs = [dacpac],
        outputs = [model_xml],
        executable = ctx.executable._builder,
        arguments = [args],
    )

    return [
        DefaultInfo(
            files = depset([model_xml]),
            runfiles = ctx.runfiles(
                files = [model_xml],
            ),
        ),
    ]

BUILDER = attr.label(
    executable = True,
    cfg = "exec",
    default = "@rules_tsql//tsql/tools/builder",
)

tsql_dacpac = rule(
    _dacpac_impl,
    attrs = {
        "srcs": attr.label_list(
            doc = """The tsql source files that comprise the DACPAC.""",
            allow_files = True,
        ),
        "properties": attr.string_dict(
            doc = """Dictionary of properties for the DacPac. Use MSBuild property format.

Values will be interpreted as MSBuild property strings as normally put in a project file.

All MSBuild sqlproj msbuild properties are supported. See [SqlBuildTask docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.tools.schema.tasks.sql.sqlbuildtask?view=sql-datatools-msbuild-16)
for details.

ex:
```python
properties = {
    "AnsiNulls": "true",
    "DefaultSchema": "custom",
    "ModelCollation": "1033, CI",
    "ValidateCasingOnIdentifiers": "true",
}
```
""",
        ),
        "_builder": BUILDER,
    },
)

tsql_unpack = rule(
    _unpack_impl,
    attrs = {
        "dacpac": attr.label(
            mandatory = True,
            providers = [DacpacInfo],
        ),
        "_builder": BUILDER,
    },
)
