using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine;
using CommandLine.Text;

namespace builder
{
    class Program
    {
        static int Main(string[] args)
        {
            // Console.WriteLine("Received: " + string.Join(" ", args));
            // var env = Environment.GetEnvironmentVariables();
            // foreach (var key in env.Keys.Cast<string>().OrderBy(k => k))
            //     Console.WriteLine($"{key}={env[key]}");

            var parser = new Parser((settings) =>
            {
                settings.HelpWriter = null;
            });

            var result = parser.ParseArguments<BuildArgs, UnpackArgs, ExtractArgs, DeployArgs>(args);
            if (result.Tag == ParserResultType.NotParsed)
            {
                var helpText = HelpText.AutoBuild(result, h =>
                {
                    h.AdditionalNewLineAfterOption = false;
                    h.Heading = "";
                    h.AutoHelp = false;
                    h.AutoVersion = false;
                    h.Copyright = "";
                    return HelpText.DefaultParsingErrorsHandler(result, h);
                }, e => e);
                Console.WriteLine(helpText);
                return 1;
            }

            return result.MapResult(
                (BuildArgs typedArgs) => Build(typedArgs),
                (UnpackArgs typedArgs) => Unpack(typedArgs),
                (ExtractArgs typedArgs) => Extract(typedArgs),
                (DeployArgs typedArgs) => Deploy(typedArgs),
                errors => 1);
        }

        private static int Deploy(DeployArgs typedArgs)
        {
            var deployer = new Deployer(typedArgs, new TaskUtil());
            var result = deployer.Deploy();
            return result ? 0 : 1;
        }

        private static int Build(BuildArgs buildArgs)
        {
            var builder = new Builder(buildArgs, new BuildOptions(), new TaskUtil());
            var result = builder.Build();
            return result ? 0 : 1;
        }

        private static int Extract(ExtractArgs args)
        {
            var extractor = new Extractor(args, new TaskUtil());
            return extractor.Extract();
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
    }

    public class BuildOptions
    {
        public bool TreatTSqlWarningsAsErrors { get; set; }
    }
}