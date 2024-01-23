using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.sqlserver
{
    [TestClass()]
    public class ConfigServiceTests_sqlserver: ConfigServiceTests
    {
        string conn = "TrustServerCertificate=True;Persist Security Info = False; User ID =dev; Password =dev; Initial Catalog =agile_config_test; Server =.";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","sqlserver" },
                {"db:conn",conn }
            });
        }
    }
}