load(":providers.bzl", "DacpacInfo")

def tsql_dacpac_macro(name, extract_args = [], **kwargs):
    tsql_dacpac(
        name = name,
        **kwargs
    )

    tsql_unpack(
        name = name + ".unpack",
        dacpac = ":" + name,
    )

    native.sh_binary(
        name = name + ".extract",
        srcs = ["@rules_tsql//tsql/tools/builder:script.sh"],
        args = [
            "extract",
            "--database_name",
            name,
            "--output_directory",
            native.package_name(),
        ] + extract_args,
        deps = ["@bazel_tools//tools/bash/runfiles"],
        data = ["@rules_tsql//tsql/tools/builder:builder_tool"],
    )

def _dacpac_impl(ctx):
    toolchain = ctx.toolchains["@rules_tsql//tsql:toolchain_type"]
    dacpac = ctx.actions.declare_file(ctx.attr.name + ".dacpac")
    model_xml = ctx.actions.declare_file("_%s/Model.xml" % ctx.attr.name)

    deps = [d[DacpacInfo].dacpac for d in ctx.attr.deps]

    args = ctx.actions.args()
    args.add_all([
        "build",
        "--label",
        str(ctx.label),
        "--output",
        dacpac.path,
    ])

    inputs = ctx.files.srcs + deps

    properties = getattr(ctx.attr, "properties", None)
    if properties:
        properties_file = ctx.actions.declare_file(ctx.attr.name + ".properties.json")
        inputs.append(properties_file)
        ctx.actions.write(properties_file, json.encode(properties))
        args.add_all([
            "--properties_file",
            properties_file,
        ])

    if len(deps) > 0:
        args.add("--deps")
        args.add(",".join([d.path for d in deps]))

    args.add("--srcs")
    args.add_all(ctx.files.srcs)

    basename = toolchain.builder.executable.path
    if basename.endswith(".exe"):
        basename = basename[:-4]

    runfiles_dir = basename + ".dll.runfiles"

    ctx.actions.run(
        mnemonic = "CompileDacpac",
        inputs = depset(inputs, transitive = [toolchain.builder.files]),
        outputs = [dacpac, model_xml],
        executable = toolchain.builder.executable,
        env = {
            "DOTNET_CLI_HOME": toolchain.dotnet_runtime.cli_home,
            "DOTNET_RUNTIME_BIN": toolchain.dotnet_runtime.dotnet,
            "RUNFILES_DIR": runfiles_dir,
        },
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
        OutputGroupInfo(
            all = [dacpac, model_xml],
        ),
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
        "deps": attr.label_list(
            doc = """List of dacpacs that this dacpac depends on.""",
            providers = [DacpacInfo],
        ),
    },
    toolchains = ["@rules_tsql//tsql:toolchain_type"],
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
