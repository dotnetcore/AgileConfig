using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Testcontainers.MySql;
using System.Data;
using AgileConfig.Server.Data.Entity;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass]
    public class SysLogServiceTests_mysql : SysLogServiceTests
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
            var sql =  this.GetFreeSql();

            sql.Delete<SysLog>().Where("1=1").ExecuteAffrows();
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