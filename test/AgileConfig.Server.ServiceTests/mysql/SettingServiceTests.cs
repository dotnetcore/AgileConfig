﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;
using System;
using Testcontainers.MySql;

namespace AgileConfig.Server.ServiceTests.mysql
{
    [TestClass]
    public class SettingServiceTests_mysql : SettingServiceTests
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