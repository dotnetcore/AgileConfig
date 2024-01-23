using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.oracle
{
    [TestClass()]
    public class ConfigServiceTests_oracle : ConfigServiceTests
    {
        string conn = "user id=x;password=x;data source=192.168.0.123/orcl";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","oracle" },
                {"db:conn",conn }
            });
        }
    }
}