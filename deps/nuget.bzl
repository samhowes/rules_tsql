load("@rules_msbuild//deps:public_nuget.bzl", "FRAMEWORKS", "PACKAGES")
load("@rules_msbuild//dotnet:defs.bzl", "nuget_deps_helper", "nuget_fetch")

def nuget_deps():
    nuget_fetch(
        name = "nuget",
        packages = {
        },
        target_frameworks = ["net5.0"],
        use_host = True,
        deps = nuget_deps_helper(FRAMEWORKS, PACKAGES),
    )
