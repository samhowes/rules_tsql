load("@bazel_skylib//lib:shell.bzl", "shell")
load("@bazel_skylib//rules:write_file.bzl", "write_file")

def build_test(name, expectations):
    target = name.rsplit("_", 1)[0]

    spec_name = name + ".spec"
    write_file(
        name = spec_name,
        out = spec_name + ".json",
        content = [json.encode(expectations)],
    )

    native.sh_test(
        name = name,
        srcs = [
            "//tests/tools/build_test:build_test.sh",
        ],
        args = [
            "$(rootpath //tests/tools/build_test)",
            "$(location :%s)" % target,
            "$(location :%s)" % spec_name,
        ],
        data = [
            "//tests/tools/build_test",
            ":" + target,
            ":" + spec_name,
        ],
        deps = ["@bazel_tools//tools/bash/runfiles"],
    )
