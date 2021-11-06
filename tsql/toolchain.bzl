load("@rules_tsql//dotnet:repository.bzl", "DotnetRuntimeInfo")
load("@rules_msbuild//dotnet/private:providers.bzl", "DotnetPublishInfo")

def _impl(ctx):
    info = ctx.attr.builder[DotnetPublishInfo]
    return [platform_common.ToolchainInfo(
        builder = struct(
            files = info.files,
            executable = info.launcher,
        ),
        dotnet_runtime = ctx.attr.dotnet_runtime[DotnetRuntimeInfo],
    )]

tsql_toolchain = rule(
    _impl,
    attrs = {
        "builder": attr.label(
            mandatory = True,
            cfg = "host",
        ),
        "dotnet_runtime": attr.label(
            mandatory = True,
            providers = [DotnetRuntimeInfo],
        ),
    },
)
