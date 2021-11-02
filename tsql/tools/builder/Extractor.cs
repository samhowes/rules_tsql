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

        public Extractor(ExtractArgs args)
        {
            _args = args;
        }

        public int Extract()
        {
            var connectionString = new SqlConnectionStringBuilder(_args.ConnectionString ?? "");
            if (!string.IsNullOrEmpty(_args.DatabaseName))
            {
                connectionString.InitialCatalog = _args.DatabaseName;
            }
            else if (string.IsNullOrEmpty(connectionString.InitialCatalog))
            {
                Console.WriteLine("Either --database_name must be provided or Initial Catalog must be specified in " +
                                  "--connection_string");
                return 1;
            }

            if (!string.IsNullOrEmpty(_args.Server))
            {
                connectionString.DataSource = _args.Server;
            }
            
            connectionString.DataSource ??= "localhost";
            
            if (!string.IsNullOrEmpty(_args.Username))
            {
                connectionString.UserID = _args.Username;
            }

            if (!string.IsNullOrEmpty(_args.Password))
            {
                connectionString.Password = _args.Password;
            }
            
            if (_args.TargetPath == null)
            {
                _args.TargetPath = Directory.GetCurrentDirectory();
            }
            else
            {
                _args.TargetPath = Path.GetFullPath(_args.TargetPath);
            }
            
            Console.WriteLine($"Using connection string: `{connectionString.ToString().Replace(_args.Password ?? "", "***")}`");
            
            var services = new DacServices(connectionString.ToString());

            string workDirectory;
            if (_args.Delete)
            {
                Console.WriteLine("--delete specified, cleaning sql files...");
                CleanFiles(_args.TargetPath);
                if (!Directory.Exists(_args.TargetPath))
                    Directory.CreateDirectory(_args.TargetPath);
                workDirectory = _args.TargetPath;
            }
            else
            {
                workDirectory = Path.Combine(_args.TargetPath, "_work");
                if (Directory.Exists(workDirectory))
                    Directory.Delete(workDirectory, true);
                Directory.CreateDirectory(workDirectory);    
            }
            
            services.Extract(workDirectory, _args.DatabaseName, "bazel", Version.Parse("0.0.1"), 
                tables:null,
                extractOptions:new DacExtractOptions()
                {
                    ExtractTarget = _args.Mode,
                });

            if (!_args.Delete)
            {
                FixFiles(_args.TargetPath, workDirectory, workDirectory);
                Directory.Delete(workDirectory, true);    
            }
            
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
                CleanFiles(subDirectory);
            }
            
            if (Directory.GetFileSystemEntries(path).Length == 0)
                Directory.Delete(path);
        }

        private void FixFiles(string targetDirectory, string workDirectory, string directory)
        {
            string Rel(string path) => path[(workDirectory.Length+1)..];
            
            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                var rel = Rel(subDirectory);
                if (!Directory.Exists(Path.Combine(targetDirectory, rel)))
                    Directory.CreateDirectory(rel);
                
                FixFiles(targetDirectory, workDirectory, subDirectory);
            }

            foreach (var file in Directory.EnumerateFiles(directory))
            {
                var rel = Rel(file);
                var dest = Path.Combine(targetDirectory, rel);
                Console.WriteLine($"{dest}");
                File.Move(file, dest, true);
            }
        }
    }
}