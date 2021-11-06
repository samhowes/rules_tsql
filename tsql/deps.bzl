load("@bazel_tools//tools/build_defs/repo:utils.bzl", "maybe")
load("@rules_tsql//dotnet:repository.bzl", "local_dotnet_runtime")

def rules_tsql_dependencies():
    maybe(
        local_dotnet_runtime,
        "dotnet_runtime",
    )
