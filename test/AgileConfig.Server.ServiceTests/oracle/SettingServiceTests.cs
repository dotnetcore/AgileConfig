using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.ServiceTests.oracle
{
    [TestClass()]
    public class SettingServiceTests_oracle : SettingServiceTests
    {
        string conn = "user id=x;password=x;data source=192.168.0.123/orcl";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "oracle";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}