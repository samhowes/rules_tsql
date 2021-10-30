load("@bazel_skylib//lib:shell.bzl", "shell")

def build_test(name, expectations):
    target = name.rsplit("_", 1)[0]

    native.sh_test(
        name = name,
        srcs = [
            "//tests/tools/build_test:build_test.sh",
        ],
        args = [
            "$(rootpath //tests/tools/build_test)",
            "$(location :%s)" % target,
            shell.quote(json.encode(expectations)),
        ],
        data = [
            "//tests/tools/build_test",
            ":" + target,
        ],
        deps = [],
    )
