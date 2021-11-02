load("@rules_msbuild//deps:public_nuget.bzl", "FRAMEWORKS", "PACKAGES")
load("@rules_msbuild//dotnet:defs.bzl", "nuget_deps_helper", "nuget_fetch")
load(":public.nuget.bzl", "TSQL_FRAMEWORKS", "TSQL_PACKAGES")

def nuget_deps():
    nuget_fetch(
        name = "nuget",
        packages = {
            "CommandLineParser/2.8.0": ["net5.0"],
            "FluentAssertions/6.2.0": ["net5.0"],
            "Microsoft.Build.Utilities.Core/16.11.0": ["net5.0"],
            "Microsoft.SqlServer.DacFx/150.5290.2-preview": ["net5.0"],
            "Newtonsoft.Json/13.0.1": ["net5.0"],
            "RulesMSBuild.Runfiles/": ["net5.0"],
            "SharpZipLib/1.3.3": ["net5.0"],
        },
        target_frameworks = ["net5.0"],
        use_host = True,
        deps = nuget_deps_helper(FRAMEWORKS, PACKAGES),
    )
