using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    public class SettingServiceTests_mongo : SettingServiceTests
    {
        public override void ClearData()
        {
        }

        string conn = "mongodb://192.168.0.125:27017/agile_config_1";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","mongodb" },
                {"db:conn",conn }
            });
        }
    }
}