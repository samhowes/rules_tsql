using System;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace builder
{
    public class Extractor
    {
        private readonly ExtractArgs _args;
        private string _workDirectory;
        private readonly TaskUtil _taskUtil;

        public Extractor(ExtractArgs args, TaskUtil taskUtil)
        {
            _args = args;
            _taskUtil = taskUtil;
        }

        public int Extract()
        {
            if (!_args.TryGetConnectionString(out var connectionString)) return 1;

            if (_args.TargetPath == null)
            {
                _args.TargetPath = Directory.GetCurrentDirectory();
            }
            else
            {
                _args.TargetPath = Path.GetFullPath(_args.TargetPath);
            }

            var services = new DacServices(connectionString.ToString());

            _workDirectory = Path.GetFullPath(Path.Combine(_args.TargetPath, "_work"));
            if (Directory.Exists(_workDirectory))
                Directory.Delete(_workDirectory, true);
            Directory.CreateDirectory(_workDirectory);

            var options = new DacExtractOptions();
            _taskUtil.SetProperties(options, _args.PropertiesFile);
            options.ExtractTarget = _args.Mode;
            services.Extract(_workDirectory, _args.DatabaseName, "bazel", Version.Parse("0.0.1"),
                tables: null,
                extractOptions: options);

            if (_args.Delete)
                CleanFiles(_args.TargetPath);
            FixFiles(_workDirectory);
            Directory.Delete(_workDirectory, true);

            return 0;
        }

        private void CleanFiles(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.sql");
            foreach (var file in files)
            {
                Console.WriteLine($"Deleting: {file}");
                File.Delete(file);
            }

            foreach (var subDirectory in Directory.EnumerateDirectories(path))
            {
                if (subDirectory == _workDirectory) continue;
                CleanFiles(subDirectory);
            }

            if (Directory.GetFileSystemEntries(path).Length == 0)
                Directory.Delete(path);
        }

        private void FixFiles(string directory)
        {
            string Rel(string path) => path[(_workDirectory.Length + 1)..];

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                var rel = Rel(subDirectory);
                var full = Path.Combine(_args.TargetPath, rel);
                if (!Directory.Exists(full))
                    Directory.CreateDirectory(full);

                FixFiles(subDirectory);
            }

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var rel = Rel(file);
                var dest = Path.Combine(_args.TargetPath, rel);
                Console.WriteLine($"{dest}");
                File.Move(file, dest, true);
            }
        }
    }
}