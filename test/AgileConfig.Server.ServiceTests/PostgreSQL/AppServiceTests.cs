using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.PostgreSQL
{
    [TestClass()]
    public class AppServiceTests_pg : AppServiceTests
    {
        string conn = "Host=192.168.0.125;Port=15432;Database=agileconfig;Username=postgres;Password=123456";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            var dict = new Dictionary<string, string>();
            dict["db:provider"] = "pg";
            dict["db:conn"] = conn;


            return Task.FromResult(dict);
        }

    }
}