load(":providers.bzl", "DacpacInfo")
load(":util.bzl", "to_manifest_path")
load("@bazel_skylib//lib:shell.bzl", "shell")

def tsql_dacpac_macro(
        name,
        connection_args = [],
        deploy_args = [],
        deploy_properties = {},
        extract_args = [],
        extract_properties = {},
        **kwargs):
    tsql_dacpac(
        name = name,
        **kwargs
    )

    tsql_extract(
        name = name + ".extract.sh",
        builder_args = [
            "--database_name",
            name,
            "--output_directory",
            native.package_name(),
        ] + connection_args + extract_args,
        properties = extract_properties,
    )
    native.sh_binary(
        name = name + ".extract",
        srcs = [name + ".extract.sh"],
    )

    tsql_deploy(
        name = name + ".deploy.sh",
        dacpac = ":" + name,
        builder_args = [
            "--database_name",
            name,
        ] + connection_args + deploy_args,
        properties = deploy_properties,
    )
    native.sh_binary(
        name = name + ".deploy",
        srcs = [name + ".deploy.sh"],
    )

def _extract_impl(ctx):
    return _util_script(ctx, "extract", [], [])

def _deploy_impl(ctx):
    dacpac = ctx.attr.dacpac[DacpacInfo].dacpac
    return _util_script(ctx, "deploy", [
        "--dacpac",
        to_manifest_path(ctx, dacpac),
    ], [dacpac])

def _util_script(ctx, action, runfiles_args, runfiles):
    toolchain = ctx.toolchains["@rules_tsql//tsql:toolchain_type"]
    script = ctx.actions.declare_file(ctx.attr.name)
    pinputs, pargs = _add_properties(ctx, True)

    ctx.actions.expand_template(
        template = ctx.file._script_template,
        output = script,
        substitutions = {
            "@@builder@@": to_manifest_path(ctx, toolchain.builder.executable),
            "@@args@@": shell.array_literal([action] + ctx.attr.builder_args),
            "@@runfiles_args@@": shell.array_literal(runfiles_args + pargs),
        },
        is_executable = True,
    )

    return [DefaultInfo(
        files = depset([script]),
        runfiles = ctx.runfiles(files = [
            script,
            toolchain.builder.executable,
            ctx.file._runfiles,
        ] + pinputs + runfiles),
        executable = script,
    )]

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

    pinputs, pargs = _add_properties(ctx, False)
    args.add_all(pargs)
    inputs.extend(pinputs)

    if len(deps) > 0:
        args.add("--deps")
        args.add_all(deps)

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

def _add_properties(ctx, use_manifest_path):
    properties = getattr(ctx.attr, "properties", None)
    if not properties:
        return ([], [])
    properties_file = ctx.actions.declare_file(ctx.attr.name + ".properties.json")
    ctx.actions.write(properties_file, json.encode_indent(properties))
    return ([properties_file], [
        "--properties_file",
        to_manifest_path(ctx, properties_file) if use_manifest_path else properties_file,
    ])

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

tsql_extract = rule(
    _extract_impl,
    attrs = {
        "builder_args": attr.string_list(),
        "properties": attr.string_dict(),
        "_script_template": attr.label(default = "@rules_tsql//tsql/tools/builder:script.sh", allow_single_file = True),
        "_runfiles": attr.label(default = "@bazel_tools//tools/bash/runfiles", allow_single_file = True),
    },
    toolchains = ["@rules_tsql//tsql:toolchain_type"],
    executable = True,
)
tsql_deploy = rule(
    _deploy_impl,
    attrs = {
        "builder_args": attr.string_list(),
        "dacpac": attr.label(
            mandatory = True,
            providers = [DacpacInfo],
        ),
        "properties": attr.string_dict(),
        "_script_template": attr.label(default = "@rules_tsql//tsql/tools/builder:script.sh", allow_single_file = True),
        "_runfiles": attr.label(default = "@bazel_tools//tools/bash/runfiles", allow_single_file = True),
    },
    toolchains = ["@rules_tsql//tsql:toolchain_type"],
    executable = True,
)
