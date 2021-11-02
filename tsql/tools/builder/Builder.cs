using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Data.Tools.Schema.Extensibility;
using Microsoft.Data.Tools.Schema.Tasks.Sql;
using Microsoft.SqlServer.Dac.Model;
using RulesMSBuild.Tools.Bazel;

namespace builder
{
    public class MSBuildBuilder
    {
        private readonly BuildArgs _args;
        private readonly BuildOptions _options;

        public MSBuildBuilder(BuildArgs args, BuildOptions options)
        {
            _args = args;
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

            var label = new Label(_args.Label);

            var outputDirectory = Path.GetDirectoryName(_args.Output);
            var buildTask = new SqlBuildTask()
            {
                IntermediateDirectory = Path.Combine(outputDirectory!, "_" + label.Name),
                OutputDirectory = outputDirectory,
                BuildEngine = new MSBuildEngine(Directory.GetCurrentDirectory()),
                DatabaseSchemaProviderName = schemaProvider.FullName,
                Source = _args.Srcs.Select(s => (ITaskItem) new TaskItem(s)).ToArray(),
                SqlTarget = new TaskItem(_args.Output)
            };
            
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
                        });
                    }).ToArray();
            }

            if (Directory.Exists(buildTask.IntermediateDirectory))
                Directory.Delete(buildTask.IntermediateDirectory);
            Directory.CreateDirectory(buildTask.IntermediateDirectory);

            if (!string.IsNullOrEmpty(_args.PropertiesFile))
            {
                if (!SetProperties(buildTask))
                    return false;
            }
            
            var result = buildTask.Execute();
            if (!result)
            {
                Console.WriteLine("Compile dacpac FAILED");
            }

            return result;
        }

        private bool SetProperties(SqlBuildTask buildTask)
        {
            var dict =
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_args.PropertiesFile));
            
            var properties = typeof(SqlBuildTask).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var (name, value) in dict!)
            {
                if (!properties.TryGetValue(name, out var property))
                {
                    Console.WriteLine($"Property `{name}` does not exist on {nameof(SqlBuildTask)}.");
                    return false;
                }
                
                if (property.GetValue(buildTask) != null) continue;

                var type = property.PropertyType;
                object propertyValue = null;
                if (type == typeof(string))
                {
                    propertyValue = value;
                }
                else if (type == typeof(TaskItem[]))
                {
                    propertyValue = value.Split(",").Select(v => new TaskItem(v));
                }
                else
                {
                    Console.WriteLine($"Warning: unknown property type '{type.Name}', can't set property '{name}'");
                }
                
                property.SetValue(buildTask, propertyValue);
                
            }

            return true;
        }
    }
}