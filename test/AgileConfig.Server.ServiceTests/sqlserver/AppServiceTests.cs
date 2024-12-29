using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.sqlserver
{
    public class AppServiceTests_sqlserver : AppServiceTests
    {
        string conn = "TrustServerCertificate=True;Persist Security Info = False; User ID =dev; Password =dev; Initial Catalog =agile_config_test; Server =.";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            var dict = new Dictionary<string, string>();
            dict["db:provider"] = "sqlserver";
            dict["db:conn"] = conn;


            return Task.FromResult(dict);
        }
    }
}