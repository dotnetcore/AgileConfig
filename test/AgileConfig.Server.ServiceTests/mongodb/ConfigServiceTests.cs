using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MongoDb;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass]
public class ConfigServiceTests_mongo : ConfigServiceTests
{
    private static readonly MongoDbContainer _container = new MongoDbBuilder().WithImage("mongo:6.0").Build();

    public override void ClearData()
    {
        var repository = new ConfigRepository(GetConn());
        var entities = repository.AllAsync().Result;
        foreach (var entity in entities) repository.DeleteAsync(entity).Wait();

        var configPublishedRepository = new ConfigPublishedRepository(GetConn());
        var configPublisheds = configPublishedRepository.AllAsync().Result;
        foreach (var entity in configPublisheds) configPublishedRepository.DeleteAsync(entity).Wait();

        var detailRepository = new PublishDetailRepository(GetConn());
        var details = detailRepository.AllAsync().Result;
        foreach (var entity in details) detailRepository.DeleteAsync(entity).Wait();

        var app_repository = new AppRepository(GetConn());
        var apps = app_repository.AllAsync().Result;
        foreach (var entity in apps) app_repository.DeleteAsync(entity).Wait();

        var appref_repository = new AppInheritancedRepository(GetConn());
        var apprefs = appref_repository.AllAsync().Result;
        foreach (var entity in apprefs) appref_repository.DeleteAsync(entity).Wait();
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

    private string GetConn()
    {
        return _container.GetConnectionString();
    }

    public override Task<Dictionary<string, string>> GetConfigurationData()
    {
        var connstr = GetConn();
        Console.WriteLine($"MongoDbContainer connstr: {connstr}");

        var dict = new Dictionary<string, string>();
        dict["db:provider"] = "mongodb";
        dict["db:conn"] = connstr;

        return Task.FromResult(dict);
    }
}