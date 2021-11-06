load("@bazel_tools//tools/build_defs/repo:utils.bzl", "maybe")
load("@bazel_tools//tools/build_defs/repo:git.bzl", "git_repository")

def rules_tsql_dependencies():
    maybe(
        git_repository,
        name = "rules_dotnet_runtime",
        commit = "3480356f4ab70015b99207d7a724ca1d24323093",  # branch main as of 2021-11-06
        shallow_since = "1636220163 -0400",
        remote = "https://github.com/samhowes/rules_dotnet_runtime",
    )
