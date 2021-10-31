using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace builder
{
    public class Builder
    {
        private readonly Args _args;
        private readonly BuildOptions _options;
        private readonly HashSet<int> _suppressedWarnings = new();
        private readonly Dictionary<string, HashSet<int>> _suppressedFileWarnings = new();
        private int _validationErrors;

        public Builder(Args args, BuildOptions options)
        {
            _args = args;
            _options = options;
        }

        public bool Build()
        {
            var model = new TSqlModel(Enum.Parse<SqlServerVersion>(_args.SqlServerVersion), new TSqlModelOptions());
            var objectOptions = new TSqlObjectOptions();
            foreach (var src in _args.Srcs)
            {
                var script = File.ReadAllText(src);
                try
                {
                    model.AddObjects(script, objectOptions);
                }
                catch (DacModelException)
                {
                    var errors = model.GetModelErrors();
                    foreach (var error in errors)
                    {
                        var level = error.Severity == ModelErrorSeverity.Error ? "Error" : "Warning";
                        Console.WriteLine($"{src}:{error.Line}:{error.Column}: {level} {error.Prefix}{error.ErrorCode}: {error.Message}");    
                    }
                    
                    Console.WriteLine("Build model failed");
                    return false;
                }
            }

            if (!ValidateModel(model)) return false;

            DacPackageExtensions.BuildPackage(
                _args.Output, 
                model, 
                new PackageMetadata() {Name = _args.Label},
                new PackageOptions { });

            return true;
        }

        private bool ValidateModel(TSqlModel model)
        {
            var modelErrors = model.GetModelValidationErrors(Enumerable.Empty<string>());
            foreach (var modelError in modelErrors)
            {
                if (modelError.Severity == ModelErrorSeverity.Error)
                {
                    _validationErrors++;
                    Console.WriteLine(modelError.GetOutputMessage(modelError.Severity));
                }
                else if (modelError.Severity == ModelErrorSeverity.Warning)
                {
                    ProcessWarning(modelError);
                }
                else
                {
                    Console.WriteLine(modelError.GetOutputMessage(modelError.Severity));
                }
            }

            if (_validationErrors > 0)
            {
                Console.WriteLine($"Found {_validationErrors} error(s), skip building package");
                return false;
            }

            return true;
        }

        void ProcessWarning(ModelValidationError modelError)
        {
            if (_suppressedWarnings.Contains(modelError.ErrorCode))
                return;

            if (_suppressedFileWarnings.TryGetValue(modelError.SourceName, out var suppressedFileWarnings)
                && suppressedFileWarnings.Contains(modelError.ErrorCode))
                return;

            if (_options.TreatTSqlWarningsAsErrors)
            {
                _validationErrors++;
            }

            Console.WriteLine(modelError.GetOutputMessage(_options.TreatTSqlWarningsAsErrors
                ? ModelErrorSeverity.Error
                : ModelErrorSeverity.Warning));
        }
    }
}