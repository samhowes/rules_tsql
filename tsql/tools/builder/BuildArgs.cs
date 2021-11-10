using System;
using System.Collections.Generic;
using CommandLine;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace builder
{
    [Verb("build", isDefault: true)]
    public class BuildArgs
    {
        [Option("label", Required = true)]
        public string Label { get; set; }

        [Option("output", Required = true)]
        public string Output { get; set; }

        [Option("sql_server_version")]
        public string SqlServerVersion { get; set; } = "Sql150";

        [Option("deps")]
        public IEnumerable<string> Deps { get; set; }

        [Option("srcs")]
        public IEnumerable<string> Srcs { get; set; }

        [Option("properties_file")]
        public string PropertiesFile { get; set; }
    }

    [Verb("unpack")]
    public class UnpackArgs
    {
        [Option("output", Required = true)]
        public string Output { get; set; }

        [Option("dacpac", Required = true)]
        public string Dacpac { get; set; }
    }

    public abstract class ConnectedArgs
    {
        [Option('d', "database_name")]
        public string DatabaseName { get; set; }

        [Option('s', "server")]
        public string Server { get; set; }

        [Option('u', "username")]
        public string Username { get; set; }

        [Option('p', "password")]
        public string Password { get; set; }

        [Option("connection_string")]
        public string ConnectionString { get; set; }

        public bool TryGetConnectionString(out SqlConnectionStringBuilder connectionString)
        {
            connectionString = new SqlConnectionStringBuilder(ConnectionString ?? "");
            if (!string.IsNullOrEmpty(DatabaseName))
            {
                connectionString.InitialCatalog = DatabaseName;
            }
            else if (string.IsNullOrEmpty(connectionString.InitialCatalog))
            {
                Console.WriteLine("Either --database_name must be provided or Initial Catalog must be specified in " +
                                  "--connection_string");
                return false;
            }

            if (!string.IsNullOrEmpty(Server))
            {
                connectionString.DataSource = Server;
            }

            connectionString.DataSource ??= "localhost";

            if (!string.IsNullOrEmpty(Username))
            {
                connectionString.UserID = Username;
            }

            if (!string.IsNullOrEmpty(Password))
            {
                connectionString.Password = Password;
            }
            else
                Password = null;

            Console.WriteLine(
                $"Using connection string: `{connectionString.ToString().Replace(Password ?? "*~*", "***")}`");
            return true;
        }
    }

    [Verb("extract")]
    public class ExtractArgs : ConnectedArgs
    {
        [Option('o', "output_directory")]
        public string TargetPath { get; set; }

        [Option("mode", Default = DacExtractTarget.SchemaObjectType)]
        public DacExtractTarget Mode { get; set; }

        [Option("delete")]
        public bool Delete { get; set; }

        [Option("properties_file")]
        public string PropertiesFile { get; set; }
    }

    [Verb("deploy")]
    public class DeployArgs : ConnectedArgs
    {
        [Option("properties_file")]
        public string PropertiesFile { get; set; }

        [Option("publish_profile")]
        public string PublishProfile { get; set; }

        [Option("dacpac", Required = true)]
        public string Dacpac { get; set; }
    }
}