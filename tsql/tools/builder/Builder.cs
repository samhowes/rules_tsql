using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Tools.Schema.Extensibility;
using Microsoft.Data.Tools.Schema.Tasks.Sql;
using Microsoft.SqlServer.Dac.Model;

namespace builder
{
    public class MSBuildBuilder
    {
        private readonly BuildArgs _buildArgs;
        private readonly BuildOptions _options;

        public MSBuildBuilder(BuildArgs buildArgs, BuildOptions options)
        {
            _buildArgs = buildArgs;
            _options = options;
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

            var outputDirectory = Path.GetDirectoryName(_buildArgs.Output);
            var buildTask = new SqlBuildTask()
            {
                IntermediateDirectory = outputDirectory,
                OutputDirectory = outputDirectory,
                BuildEngine = new MSBuildEngine(Directory.GetCurrentDirectory()),
                DatabaseSchemaProviderName = schemaProvider.FullName,
                Source = _buildArgs.Srcs.Select(s => (ITaskItem)new TaskItem(s)).ToArray(),
                SqlTarget = new TaskItem(_buildArgs.Output)
            };

            // var properties = typeof(SqlBuildTask).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            var result = buildTask.Execute();
            if (!result)
            {
                Console.WriteLine("Compile dacpac FAILED");
            }
            return result;
        }
    }
}