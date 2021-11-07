load("@rules_dotnet_runtime//dotnet:defs.bzl", "DotnetPublishInfo", "DotnetRuntimeInfo")

def _builder_impl(ctx):
    executable = ctx.file.launcher if ctx.configuration.host_path_separator == ":" else ctx.file.launcher_windows
    files = depset(ctx.files.files)
    return [DotnetPublishInfo(
        launcher = ctx.file.launcher,
        launcher_windows = ctx.file.launcher_windows,
        files = files,
    ), DefaultInfo(
        files = files,
        runfiles = ctx.runfiles(transitive_files = files),
    )]

builder = rule(
    _builder_impl,
    attrs = {
        "launcher": attr.label(allow_single_file = True),
        "launcher_windows": attr.label(allow_single_file = True),
        "files": attr.label(allow_files = True),
    },
)
