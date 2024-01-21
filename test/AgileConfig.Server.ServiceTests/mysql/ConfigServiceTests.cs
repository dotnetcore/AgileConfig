using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.Service.Tests;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass()]
    public class ConfigServiceTests_mysql : ConfigServiceTests
    {
        string conn = "Database=agile_config_test;Data Source=192.168.0.125;User Id=root;Password=x;port=13306";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "mysql";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}