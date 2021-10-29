
def _dacpac_impl(ctx):
    pass

tsql_dacpac = rule(
    _dacpac_impl,
    attrs = {
        "srcs": attr.label_list(
            doc = """The tsql source files that comprise the DACPAC.""",
            allow_files = True,
        ),
        "_builder": attr.label(default = "@rules_tsql//tsql/tools/builder")
    }
)