load("@rules_dotnet_runtime//dotnet:defs.bzl", "DotnetPublishInfo")

def _builder_impl(ctx):
    return [DotnetPublishInfo(
        launcher = ctx.file.launcher,
        files = depset(ctx.files.files),
    )]

builder = rule(
    _builder_impl,
    attrs = {
        "launcher": attr.label(allow_single_file = True),
        "files": attr.label(allow_files = True),
    },
)
