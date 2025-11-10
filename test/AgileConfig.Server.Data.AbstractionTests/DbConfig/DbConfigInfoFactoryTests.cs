using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Data.AbstractionTests.DbConfig;

[TestClass]
public class DbConfigInfoFactoryTests
{
    [TestMethod]
    public void TestGetConfigInfo()
    {
        var configMap = new Dictionary<string, string>
        {
            { "db:provider", "sqlserver" },
            { "db:conn", "localhost" },
            { "db:env:test:provider", "sqlite" },
            { "db:env:test:conn", "Data Source=agile_config.db" }
        };

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(configMap);
        var configuration = configurationBuilder.Build();
        Global.Config = configuration;

        var configInfo = new DbConfigInfoFactory(configuration).GetConfigInfo();
        Assert.IsNotNull(configInfo);
        Assert.AreEqual("sqlserver", configInfo.Provider);
        Assert.AreEqual("localhost", configInfo.ConnectionString);

        configInfo = new DbConfigInfoFactory(configuration).GetConfigInfo("test");
        Assert.IsNotNull(configInfo);
        Assert.AreEqual("sqlite", configInfo.Provider);
        Assert.AreEqual("Data Source=agile_config.db", configInfo.ConnectionString);

        configInfo = new DbConfigInfoFactory(configuration).GetConfigInfo("x");
        Assert.IsNotNull(configInfo);
        Assert.AreEqual("sqlserver", configInfo.Provider);
        Assert.AreEqual("localhost", configInfo.ConnectionString);
    }
}