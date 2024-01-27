using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using System.Threading.Tasks;
using System;
using Testcontainers.MongoDb;
using AgileConfig.Server.Data.Repository.Mongodb;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    [TestClass()]
    public class AppServiceTests_mongo : AppServiceTests
    {
        static MongoDbContainer _container = new MongoDbBuilder().WithImage("mongo:6.0").Build();

        public override void ClearData()
        {
            var app_repository = new AppRepository(_container.GetConnectionString());
            var apps = app_repository.AllAsync().Result;
            foreach (var entity in apps)
            {
                app_repository.DeleteAsync(entity).Wait();
            }

            var appref_repository = new AppInheritancedRepository(_container.GetConnectionString());
            var apprefs = appref_repository.AllAsync().Result;
            foreach (var entity in apprefs)
            {
                appref_repository.DeleteAsync(entity).Wait();
            }
        }

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