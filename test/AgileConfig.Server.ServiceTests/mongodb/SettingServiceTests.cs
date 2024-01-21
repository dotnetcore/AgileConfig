using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    [TestClass()]
    public class SettingServiceTests_mongo : SettingServiceTests
    {
        public override void ClearData()
        {
        }

        string conn = "mongodb://192.168.0.125:27017/agile_config_1";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "mongodb";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}