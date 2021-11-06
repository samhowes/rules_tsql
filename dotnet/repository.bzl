DOTNET_CLI_HOME = "DOTNET_CLI_HOME"

DotnetRuntimeInfo = provider(
    fields = {
        "dotnet": "dotnet cli executable file",
        "cli_home": "dotnet cli home",
    },
)

def _impl(ctx):
    return [DotnetRuntimeInfo(
        dotnet = ctx.attr.dotnet,
        cli_home = ctx.attr.cli_home,
    )]

def _repoistory_impl(ctx):
    dotnet_path = ctx.which("dotnet")
    if dotnet_path != None:
        dotnet = ctx.path(dotnet_path)
        cli_home = dotnet.realpath.dirname
    else:
        cli_home = ctx.os.environ.get(DOTNET_CLI_HOME, None)
        if cli_home == None:
            fail("dotnet not found on path and %s not set: download and install a dotnet runtime: https://github.com/dotnet/runtime" % DOTNET_CLI_HOME)
        suffix = ""
        if ctx.os.name.startswith("windows"):
            suffix = ".exe"
        dotnet = ctx.path(cli_home).get_child("dotnet" + suffix)
        if not dotnet.exists:
            fail("found %s via %s but file does not exist" % (dotnet, DOTNET_CLI_HOME))

    ctx.file("BUILD.bazel", """load("@rules_tsql//dotnet:repository.bzl", "dotnet_runtime")

package(default_visibility = ["//visibility:public"])

dotnet_runtime(
    name = "dotnet_runtime",
    dotnet = "{dotnet}",
    cli_home = "{cli_home}",
)

""".format(
        dotnet = dotnet.realpath,
        cli_home = dotnet.realpath.dirname,
    ))

local_dotnet_runtime = repository_rule(
    _repoistory_impl,
    configure = True,
    environ = [DOTNET_CLI_HOME],
)

dotnet_runtime = rule(
    _impl,
    attrs = {
        "dotnet": attr.string(mandatory = True, doc = "Path to the dotnet cli executable"),
        "cli_home": attr.string(mandatory = True, doc = "Path to DOTNET_CLI_HOME"),
    },
)
