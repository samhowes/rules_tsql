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

            var result = parser.ParseArguments<BuildArgs, ExtractArgs, DeployArgs>(args);
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
    }

    public class BuildOptions
    {
        public bool TreatTSqlWarningsAsErrors { get; set; }
    }
}