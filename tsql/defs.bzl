load("@bazel_tools//tools/build_defs/repo:utils.bzl", "maybe")
load("@rules_dotnet_runtime//dotnet:defs.bzl", "local_dotnet_runtime")
load("//tsql/private:dacpac.bzl", _tsql_dacpac_macro = "tsql_dacpac_macro")

tsql_dacpac = _tsql_dacpac_macro

def tsql_register_toolchains():
    maybe(
        local_dotnet_runtime,
        "dotnet_runtime",
    )
    native.register_toolchains(
        "@rules_tsql//tsql:toolchain",
    )
