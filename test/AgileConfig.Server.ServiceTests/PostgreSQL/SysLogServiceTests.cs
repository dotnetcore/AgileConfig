using Microsoft.VisualStudio.TestTools.UnitTesting;

using AgileConfig.Server.ServiceTests.sqlite;
using System.Collections.Generic;

namespace AgileConfig.Server.ServiceTests.PostgreSQL
{
    [TestClass()]
    public class SysLogServiceTests_pg: SysLogServiceTests
    {
        string conn = "Host=192.168.0.125;Port=15432;Database=agileconfig;Username=postgres;Password=123456";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "npgsql";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}