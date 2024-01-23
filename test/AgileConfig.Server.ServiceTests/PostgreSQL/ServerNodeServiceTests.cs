using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.PostgreSQL
{
    [TestClass()]
    public class ServerNodeServiceTests_pg: ServerNodeServiceTests
    {
        string conn = "Host=192.168.0.125;Port=15432;Database=agileconfig;Username=postgres;Password=123456";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","pg" },
                {"db:conn",conn }
            });
        }
    }
}