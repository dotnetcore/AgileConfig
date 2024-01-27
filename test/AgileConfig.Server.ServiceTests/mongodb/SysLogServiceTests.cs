using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MongoDb;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass()]
public class SysLogServiceTests_mongo : SysLogServiceTests
{
    public override void ClearData()
    {
        var repository = new SysLogRepository(_container.GetConnectionString());
       var syslogs = repository.AllAsync().Result;
        foreach (var log in syslogs)
        {
            repository.DeleteAsync(log).Wait();
        }
    }

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