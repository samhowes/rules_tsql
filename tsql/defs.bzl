load("//tsql/private:dacpac.bzl", _tsql_dacpac_macro = "tsql_dacpac_macro")

tsql_dacpac = _tsql_dacpac_macro

def tsql_register_toolchains():
    native.register_toolchains(
        "@rules_tsql//tsql:toolchain",
    )
