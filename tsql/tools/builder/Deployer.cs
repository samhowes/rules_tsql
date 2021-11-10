using System;
using Microsoft.Build.Utilities;
using Microsoft.Data.Tools.Schema.Tasks.Sql;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace builder
{
    public class Deployer
    {
        private readonly DeployArgs _args;
        private readonly TaskUtil _taskUtil;

        public Deployer(DeployArgs args, TaskUtil taskUtil)
        {
            _args = args;
            _taskUtil = taskUtil;
        }

        public bool Deploy()
        {
            if (!_args.TryGetConnectionString(out var connectionString)) return false;

            var services = new DacServices(connectionString.ToString());
            var options = new DacDeployOptions();

            if (!_taskUtil.SetProperties(options, _args.PropertiesFile)) return false;

            services.Message += (sender, args) => Console.WriteLine(args.Message.Message);
            services.Deploy(DacPackage.Load(_args.Dacpac), connectionString.InitialCatalog, options: options,
                upgradeExisting: true);
            return true;
        }
    }
}