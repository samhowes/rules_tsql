load("@rules_tsql//tsql:defs.bzl", _tsql_dacpac = "tsql_dacpac")

DOCKER_SERVER = "localhost"
DOCKER_USER = "sa"
DOCKER_PASSWORD = "Password1234!"

def tsql_dacpac(name, extract_args = [], deploy_args = [], **kwargs):
    _tsql_dacpac(
        name = name,
        connection_args = [
            "--server",
            DOCKER_SERVER,
            "--username",
            DOCKER_USER,
            "--password",
            DOCKER_PASSWORD,
        ],
        extract_args = [
            "--mode",
            "Schema",
            #            "--delete",
        ] + extract_args,
        deploy_args = deploy_args,
        deploy_properties = {
            "CreateNewDatabase": "True",
        },
        **kwargs
    )
