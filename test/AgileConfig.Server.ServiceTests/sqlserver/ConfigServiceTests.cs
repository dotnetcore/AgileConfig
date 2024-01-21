using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AgileConfig.Server.Service.Tests.sqlserver
{
    [TestClass()]
    public class ConfigServiceTests_sqlserver: ConfigServiceTests
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