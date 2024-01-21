using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.sqlserver
{
    [TestClass()]
    public class AppServiceTests_sqlserver : AppServiceTests
    {
        string conn = "TrustServerCertificate=True;Persist Security Info = False; User ID =dev; Password =dev; Initial Catalog =agile_config_test; Server =.";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "sqlserver";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}