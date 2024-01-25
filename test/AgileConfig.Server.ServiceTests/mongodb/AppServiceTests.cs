using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;
using System;
using Testcontainers.MongoDb;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    [TestClass()]
    public class AppServiceTests_mongo : AppServiceTests
    {
        static MongoDbContainer _container = new MongoDbBuilder().WithImage("mongo:6.0").Build();

        [ClassInitialize]
        public static async Task ClassInit(TestContext testContext)
        {
            await _container.StartAsync();
            Console.WriteLine($"MongoDbContainer started");
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _container.DisposeAsync();
            Console.WriteLine($"MongoDbContainer dispose");
        }


        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            var connstr = _container.GetConnectionString();
            Console.WriteLine($"MongoDbContainer connstr: {connstr}");

            var dict = new Dictionary<string, string>();
            dict["db:provider"] = "mongodb";
            dict["db:conn"] = connstr;

            return Task.FromResult(dict);
        }
    }
}