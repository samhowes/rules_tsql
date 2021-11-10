load("@bazel_tools//tools/build_defs/repo:utils.bzl", "maybe")
load("@bazel_tools//tools/build_defs/repo:git.bzl", "git_repository")

def rules_tsql_dependencies():
    maybe(
        git_repository,
        name = "rules_dotnet_runtime",
        commit = "62bf8eeabb19638856f53fb97898827b7de77aa4",  # branch main as of 2021-11-06
        shallow_since = "1636228333 -0400",
        remote = "https://github.com/samhowes/rules_dotnet_runtime",
    )
    maybe(
        git_repository,
        name = "bazel_skylib",
        commit = "df3c9e2735f02a7fe8cd80db4db00fec8e13d25f",  # `master` as of 2021-08-19
        remote = "https://github.com/bazelbuild/bazel-skylib",
        shallow_since = "1629300223 -0400",
    )
