using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;

namespace AgileConfig.Server.Service.Tests.PostgreSQL
{
    [TestClass()]
    public class AppServiceTests_pg : AppServiceTests
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