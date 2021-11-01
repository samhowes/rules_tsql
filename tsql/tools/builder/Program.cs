using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine;

namespace builder
{
    [Verb("build", isDefault:true)]
    public class BuildArgs
    {
        [Option("label", Required = true)]
        public string Label { get; set; }

        [Option("output", Required = true)] 
        public string Output { get; set; }

        [Option("sql_server_version")] public string SqlServerVersion { get; set; } = "Sql150";
        
        [Option("deps")]
        public IEnumerable<string> Deps { get; set; }
        
        [Option("srcs")]
        public IEnumerable<string> Srcs { get; set; }
    }

    [Verb("unpack")]
    public class UnpackArgs
    {
        [Option("output", Required = true)] 
        public string Output { get; set; }
        
        [Option("dacpac", Required = true)] 
        public string Dacpac { get; set; }
    }
    
    class Program
    {
        static int Main(string[] args)
        {
            // Console.WriteLine("Received: " + string.Join(" ", args));
            // var env = Environment.GetEnvironmentVariables();
            // foreach (var key in env.Keys.Cast<string>().OrderBy(k => k))
            //     Console.WriteLine($"{key}={env[key]}");
            
            return Parser.Default.ParseArguments<BuildArgs, UnpackArgs>(args)
                .MapResult(
                    (BuildArgs typedArgs) => Build(typedArgs),
                    (UnpackArgs typedArgs) => Unpack(typedArgs),
                    errors => 1);
        }

        private static int Unpack(UnpackArgs args)
        {
            var zip = ZipFile.OpenRead(args.Dacpac);
            var files = new Dictionary<string, ZipArchiveEntry>();
            foreach (var entry in zip.Entries)
            {
                files[entry.FullName] = entry;
            }

            if (!files.TryGetValue("model.xml", out var modelXml))
            {
                Console.WriteLine("Invalid Dacpac: model.xml not found.");
                return 1;
            }

            using var output = File.Create(args.Output);
            using var input = modelXml.Open();
            input.CopyTo(output);
            output.Flush();
            return 0;
        }

        private static int Build(BuildArgs buildArgs)
        {
            var builder = new MSBuildBuilder(buildArgs, new BuildOptions());
            var result = builder.Build();
            return result ? 0 : 1;
            
        }
    }

    public class BuildOptions
    {
        public bool TreatTSqlWarningsAsErrors { get; set; }
    }
}
