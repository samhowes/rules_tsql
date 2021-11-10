#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Tools.Schema.Extensibility;
using Microsoft.Data.Tools.Schema.Tasks.Sql;
using Microsoft.SqlServer.Dac.Model;
using RulesMSBuild.Tools.Bazel;

namespace builder
{
    public class Builder
    {
        private readonly BuildArgs _args;
        private readonly BuildOptions _options;
        private TaskUtil _taskUtil;

        public Builder(BuildArgs args, BuildOptions options, TaskUtil taskUtil)
        {
            _args = args;
            _options = options;
            _taskUtil = taskUtil;
        }

        public bool Build()
        {
            var schemaProvider = typeof(IExtension).Assembly.GetTypes()
                .Where(t => t.Name.EndsWith("DatabaseSchemaProvider"))
                .FirstOrDefault(t => t.Name.StartsWith(SqlServerVersion.Sql150.ToString()));

            if (schemaProvider == null)
            {
                Console.WriteLine("Failed to locate schema provider.");
                return false;
            }

            var label = new Label(_args.Label);

            var outputDirectory = Path.GetDirectoryName(_args.Output);
            var buildTask = new SqlBuildTask();

            var deps = _args.Deps?.ToList();
            if (deps?.Any() == true)
            {
                buildTask.SqlReferencePath =
                    deps.Select(d =>
                    {
                        var name = Path.GetFileNameWithoutExtension(d);
                        return (ITaskItem) new TaskItem(d, new Dictionary<string, string>()
                        {
                            ["Name"] = name,
                            ["DatabaseVariableLiteralValue"] = name,
                            // don't validate the external schema: it was already validated appropriately 
                            // when we built it before
                            ["SuppressMissingDependenciesErrors"] = "True",
                        });
                    }).ToArray();
            }


            if (!_taskUtil.SetProperties(buildTask, _args.PropertiesFile)) return false;

            buildTask.IntermediateDirectory = Path.Combine(outputDirectory!, "_" + label.Name);
            buildTask.OutputDirectory = outputDirectory;
            buildTask.DatabaseSchemaProviderName = schemaProvider.FullName;
            buildTask.Source = _args.Srcs.Select(s => (ITaskItem) new TaskItem(s)).ToArray();
            buildTask.SqlTarget = new TaskItem(_args.Output);

            if (Directory.Exists(buildTask.IntermediateDirectory))
                Directory.Delete(buildTask.IntermediateDirectory);
            Directory.CreateDirectory(buildTask.IntermediateDirectory);

            if (!_taskUtil.ExecuteTask(buildTask))
            {
                Console.WriteLine("Compile dacpac FAILED");
                return false;
            }

            return true;
        }
    }
}