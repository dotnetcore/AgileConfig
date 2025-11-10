using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MongoDb;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass]
public class SettingServiceTests_mongo : SettingServiceTests
{
    private static readonly MongoDbContainer _container = new MongoDbBuilder().WithImage("mongo:6.0").Build();

    public override void ClearData()
    {
    }

    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        await _container.StartAsync();
        Console.WriteLine("MongoDbContainer started");
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _container.DisposeAsync();
        Console.WriteLine("MongoDbContainer dispose");
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