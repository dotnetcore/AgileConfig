using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace AgileConfig.Server.ServiceTests.PostgreSQL
{
    [TestClass()]
    public class ConfigServiceTests_pg: ConfigServiceTests
    {
        static PostgreSqlContainer _container = new PostgreSqlBuilder().WithImage("postgres:15.1").Build();

        [ClassInitialize]
        public static async Task ClassInit(TestContext testContext)
        {
            await _container.StartAsync();
            Console.WriteLine($"PostgreSqlContainer started");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _container.DisposeAsync();
            Console.WriteLine($"PostgreSqlContainer dispose");
        }
        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            var connstr = _container.GetConnectionString();
            Console.WriteLine($"PostgreSqlContainer connstr: {connstr}");

            var dict = new Dictionary<string, string>();
            dict["db:provider"] = "pg";
            dict["db:conn"] = connstr;


            return Task.FromResult(dict);
        }

    }
}