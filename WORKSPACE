workspace(name = "rules_tsql")

load("@bazel_tools//tools/build_defs/repo:git.bzl", "git_repository")

git_repository(
    name = "bazel_skylib",
    commit = "df3c9e2735f02a7fe8cd80db4db00fec8e13d25f",  # `master` as of 2021-08-19
    remote = "https://github.com/bazelbuild/bazel-skylib",
    shallow_since = "1629300223 -0400",
)

# bzl:generated start
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")

http_archive(
    name = "rules_msbuild",
    sha256 = "607a251ed80ef195c85edd95689df96e7aae97911bbbf0be1884594d32d8472a",
    urls = ["https://github.com/samhowes/rules_msbuild/releases/download/0.0.10/rules_msbuild-0.0.10.tar.gz"],
)

load("@rules_msbuild//dotnet:deps.bzl", "msbuild_register_toolchains", "msbuild_rules_dependencies")

msbuild_rules_dependencies()

# See https://dotnet.microsoft.com/download/dotnet for valid versions
msbuild_register_toolchains(version = "host")

# bzl:generated end

load("//deps:nuget.bzl", "nuget_deps")

# gazelle:nuget_macro deps:nuget.bzl%nuget_deps
nuget_deps()

# gazelle:nuget_macro deps/nuget.bzl%nuget_deps
nuget_deps()
