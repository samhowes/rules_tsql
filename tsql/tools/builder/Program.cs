using System.Collections.Generic;
using CommandLine;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using RulesMSBuild.Tools.Bazel;

namespace builder
{
    public class Args
    {
        [Option("label", Required = true)]
        public string Label { get; set; }

        [Option("output", Required = true)] 
        public string Output { get; set; }

        [Option("sql_server_version")] public string SqlServerVersion { get; set; } = "Sql150";
        
        [Value(0)]
        public IEnumerable<string> Srcs { get; set; }
    }
    class Program
    {
        static int Main(string[] args)
        {
            // Console.WriteLine("Received: " + string.Join(" ", args));
            // var env = Environment.GetEnvironmentVariables();
            // foreach (var key in env.Keys.Cast<string>().OrderBy(k => k))
            //     Console.WriteLine($"{key}={env[key]}");
            
            return Parser.Default.ParseArguments<Args>(args)
                .MapResult(
                    (Args typedArgs) => Build(typedArgs),
                    errors => 1);
        }

        private static int Build(Args args)
        {
            var builder = new Builder(args, new BuildOptions());
            builder.Build();
            return 0;
            
        }
    }

    public class BuildOptions
    {
        public bool TreatTSqlWarningsAsErrors { get; set; }
    }
}
