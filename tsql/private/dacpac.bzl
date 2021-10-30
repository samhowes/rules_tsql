def _dacpac_impl(ctx):
    dacpac = ctx.actions.declare_file(ctx.attr.name + ".dacpac")

    args = ctx.actions.args()
    args.add_all(["--label=" + str(ctx.label), "--output=" + dacpac.path] + ctx.files.srcs)
    ctx.actions.run(
        mnemonic = "CompileDacpac",
        inputs = ctx.files.srcs,
        outputs = [dacpac],
        executable = ctx.executable._builder,
        arguments = [args],
    )
    return [
        DefaultInfo(
            files = depset([dacpac]),
            runfiles = ctx.runfiles(
                files = [dacpac],
            ),
        ),
        OutputGroupInfo(
            all = depset([dacpac]),
        ),
    ]

tsql_dacpac = rule(
    _dacpac_impl,
    attrs = {
        "srcs": attr.label_list(
            doc = """The tsql source files that comprise the DACPAC.""",
            allow_files = True,
        ),
        "_builder": attr.label(
            executable = True,
            cfg = "exec",
            default = "@rules_tsql//tsql/tools/builder",
        ),
    },
)
