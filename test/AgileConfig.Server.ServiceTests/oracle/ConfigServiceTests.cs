using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service.Tests;
using System.Collections.Generic;

namespace AgileConfig.Server.ServiceTests.oracle
{
    [TestClass()]
    public class ConfigServiceTests_oracle : ConfigServiceTests
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