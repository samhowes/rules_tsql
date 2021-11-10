using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.Build.Utilities;
using Microsoft.Data.Tools.Schema.Tasks.Sql;

namespace builder
{
    public class TaskUtil
    {
        private readonly MSBuildEngine _buildEngine;

        public TaskUtil()
        {
            _buildEngine = new MSBuildEngine(Directory.GetCurrentDirectory());
        }

        public bool ExecuteTask<TTask>(TTask task) where TTask : DataTask
        {
            task.BuildEngine = _buildEngine;
            return task.Execute();
        }

        public bool SetProperties<TTask>(TTask obj, string propertiesFile)
        {
            if (string.IsNullOrEmpty(propertiesFile))
                return true;
            var dict =
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(propertiesFile));

            var properties = typeof(TTask).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var (name, value) in dict!)
            {
                if (!properties.TryGetValue(name, out var property))
                {
                    Console.WriteLine($"Property `{name}` does not exist on {nameof(TTask)}.");
                    return false;
                }

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
                else if (type == typeof(bool))
                {
                    if (!bool.TryParse(value, out var isSet))
                    {
                        Console.WriteLine($"Error: failed to parse boolean value '{value}'");
                        return false;
                    }

                    propertyValue = isSet;
                }
                else
                {
                    Console.WriteLine($"Error: unknown property type '{type.Name}', can't set property '{name}'");
                    return false;
                }

                property.SetValue(obj, propertyValue);
            }

            return true;
        }
    }
}