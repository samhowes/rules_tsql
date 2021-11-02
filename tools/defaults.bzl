load("@rules_tsql//tsql:defs.bzl", _tsql_dacpac = "tsql_dacpac")

DOCKER_SERVER = "localhost"
DOCKER_USER = "sa"
DOCKER_PASSWORD = "Password1234!"

def tsql_dacpac(name, extract_args = [], **kwargs):
    _tsql_dacpac(
        name = name,
        extract_args = [
            "--server",
            DOCKER_SERVER,
            "--username",
            DOCKER_USER,
            "--password",
            DOCKER_PASSWORD,
            "--mode",
            "Schema",
            "--delete",
        ] + extract_args,
        **kwargs
    )
