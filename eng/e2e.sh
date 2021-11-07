#!/bin/bash

set -euo pipefail

rm -f bazel-bin/rules_tsql.tar.gz

bazel build //:tar

if [[ -d "tmp" ]]; then rm -rf tmp; fi

mkdir -p tmp
cp bazel-bin/rules_tsql.tar.gz tmp
tarsha="$(cat bazel-bin/rules_tsql.tar.gz.sha256)"

pushd tmp
tarpath="$(pwd)/rules_tsql.tar.gz"
if [[ "$(uname -s)" == *"NT"* ]]; then
  tarpath="$(cygpath -w "$tarpath")";
fi;
cat >>WORKSPACE <<EOF
workspace(name = "rules_tsql_test")
load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
http_archive(
    name = "rules_tsql",
    sha256 = "$tarsha",
    url = "file:$tarpath",
)
load("@rules_tsql//tsql:deps.bzl", "rules_tsql_dependencies")
rules_tsql_dependencies()
load("@rules_tsql//tsql:defs.bzl", "tsql_register_toolchains")
tsql_register_toolchains()
EOF

cat >>Foo.sql <<EOF
CREATE TABLE dbo.Foo (
  Id INT NOT NULL
)

EOF

cat >>BUILD.bazel <<EOF
load("@rules_tsql//tsql:defs.bzl", "tsql_dacpac")
tsql_dacpac(
  name = "foo",
  srcs = ["Foo.sql"]
)
EOF

bazel sync --only rules_tsql

export RULES_TSQL_TESTING=1
bazel build //:foo
actual="$(bazel run //:foo.extract)"

suffix=""
if [[ "$(uname -s)" == *"NT"* ]]; then suffix=".exe"; fi;

expected="external/rules_tsql/tsql/tools/builder/prebuilt/builder$suffix extract --database_name foo --output_directory"

if [[ "$actual" != *"$expected"* ]]; then
  echo "unexpected result"
  echo "    expected: $expected"
  echo "    actual: $actual"
  exit 1
fi

echo "SUCCESS"


