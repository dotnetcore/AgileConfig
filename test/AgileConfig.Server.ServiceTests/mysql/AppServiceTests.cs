using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testcontainers.MySql;

namespace AgileConfig.Server.ServiceTests.mysql;

[TestClass]
public class AppServiceTests_mysql : AppServiceTests
{
    private static readonly MySqlContainer _container = new MySqlBuilder().WithImage("mysql:8.0").Build();

    [ClassInitialize]
    public static async Task ClassInit(TestContext testContext)
    {
        await _container.StartAsync();
        Console.WriteLine("MySqlContainer started");
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _container.DisposeAsync();
        Console.WriteLine("MySqlContainer dispose");
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