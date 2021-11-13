# Initial Release

<!-- rules_tsql:snippet start -->
```python
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
http_archive(
    name = "rules_tsql",
    sha256 = "e3adf6a43a6c83346535289d8d4b829123a5cf1fca56e82f884ee30c985c3c9a",
    url = "https://github.com/samhowes/rules_tsql/releases/download/0.0.6/rules_tsql-0.0.6.tar.gz",
)
load("@rules_tsql//tsql:deps.bzl", "rules_tsql_dependencies")
rules_tsql_dependencies()
load("@rules_tsql//tsql:defs.bzl", "tsql_register_toolchains")
tsql_register_toolchains()
```
<!-- rules_tsql:snippet end -->