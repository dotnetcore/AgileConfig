using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass()]
    public class ConfigServiceTests_mysql : ConfigServiceTests
    {
        string conn = "Database=agile_config_test;Data Source=192.168.0.125;User Id=root;Password=x;port=13306";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","mysql" },
                {"db:conn",conn }
            });
        }
    }
}