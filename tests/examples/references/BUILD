load("//tsql:defs.bzl", "tsql_dacpac")
load("//tests/tools/build_test:build_test.bzl", "build_test")

# this dacpac holds the actual data
tsql_dacpac(
    name = "transitive",
    srcs = ["transitive.sql"],
)

# this dacpac queries the actual data with a view
tsql_dacpac(
    name = "provider",
    srcs = ["provide.sql"],
    deps = [
        ":transitive",
    ],
)

# this dacpac queries the view without knowing the data comes from another database
tsql_dacpac(
    name = "consumer",
    srcs = ["consume.sql"],
    deps = [
        ":provider",
    ],
)

build_test(
    name = "provider_test",
    expectations = {
        "Header": {
            "CustomData[@Category='Reference']": {
                "@Type": "SqlSchema",
                # the filename is recorded as the "Logical Name"
                "Metadata[@Name='LogicalName']/@Value": "transitive.dacpac",
                # external references are represented as "External Parts"
                "Metadata[@Name='ExternalParts']/@Value": "[transitive]",
            },
        },
    },
)

build_test(
    name = "consumer_test",
    expectations = {
        "Header": {
            "CustomData[@Category='Reference']": {
                "@Type": "SqlSchema",
                # the filename is recorded as the "Logical Name"
                "Metadata[@Name='LogicalName']/@Value": "provider.dacpac",
                # external references are represented as "External Parts"
                "Metadata[@Name='ExternalParts']/@Value": "[provider]",
            },
        },
        "Model": {
            "Element[@Name='[dbo].[consume]']": {
                "@Type": "SqlView",
                "Relationship[@Name='Columns']/Entry": {
                    "Element[@Name='[dbo].[consume].[Id]']": {
                        "@Type": "SqlComputedColumn",
                        "Relationship[@Name='ExpressionDependencies']/Entry/References": {
                            # the view depends on an external column
                            "@Name": "[provider]|[dbo].[provide].[Id]",
                            "@ExternalSource": "provider.dacpac",
                        },
                    },
                },
                "Relationship[@Name='QueryDependencies']/Entry/References": {
                    # the view depends on an external table
                    "@Name": "[provider]|[dbo].[provide]",
                    "@ExternalSource": "provider.dacpac",
                },
            },
        },
    },
)
