workspace(name = "rules_tsql")

load("@bazel_tools//tools/build_defs/repo:git.bzl", "git_repository")

git_repository(
    name = "bazel_skylib",
    commit = "df3c9e2735f02a7fe8cd80db4db00fec8e13d25f",  # `master` as of 2021-08-19
    remote = "https://github.com/bazelbuild/bazel-skylib",
    shallow_since = "1629300223 -0400",
)

load("@rules_tsql//tsql:deps.bzl", "rules_tsql_dependencies")

rules_tsql_dependencies()

load("@rules_tsql//tsql:defs.bzl", "tsql_register_toolchains")

tsql_register_toolchains()

# bzl:generated start
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
http_archive(
    name = "rules_msbuild",
    sha256 = "b969e8b635f598c4f53c2d292ca6e2216765a50c8eae87fac5bafaffcc14d603",
    urls = ["https://github.com/samhowes/rules_msbuild/releases/download/0.0.14/rules_msbuild-0.0.14.tar.gz"],
)
load("@rules_msbuild//dotnet:deps.bzl", "msbuild_register_toolchains", "msbuild_rules_dependencies")

msbuild_rules_dependencies()
# See https://dotnet.microsoft.com/download/dotnet for valid versions
msbuild_register_toolchains(version = "host")

# bzl:generated end

load("//deps:nuget.bzl", "nuget_deps")

# gazelle:nuget_macro deps:nuget.bzl%nuget_deps
nuget_deps()
