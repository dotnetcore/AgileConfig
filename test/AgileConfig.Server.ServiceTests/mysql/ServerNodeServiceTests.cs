using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;
using Testcontainers.MySql;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass]
    public class ServerNodeServiceTests_mysql : ServerNodeServiceTests
    {
        static MySqlContainer _container = new MySqlBuilder().WithImage("mysql:8.0").Build();

        [ClassInitialize]
        public static async Task ClassInit(TestContext testContext)
        {
            await _container.StartAsync();
            Console.WriteLine($"MySqlContainer started");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _container.DisposeAsync();
            Console.WriteLine($"MySqlContainer dispose");
        }

        public override void ClearData()
        {
            var sql = this.GetFreeSql();

            sql.Delete<ServerNode>().Where("1=1").ExecuteAffrows();
        }


        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            var connstr = _container.GetConnectionString();
            Console.WriteLine($"MySqlContainer connstr: {connstr}");

            var dict = new Dictionary<string, string>();
            dict["db:provider"] = "mysql";
            dict["db:conn"] = connstr;

            return Task.FromResult(dict);
        }
    }
}