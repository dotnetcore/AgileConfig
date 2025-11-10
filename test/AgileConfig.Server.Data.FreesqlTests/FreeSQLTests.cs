using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Data.Freesql.Tests;

[TestClass]
public class FreeSQLTests
{
    [TestMethod]
    public void GetInstanceByEnvTest()
    {
        var configMap = new Dictionary<string, string>
        {
            { "db:provider", "sqlite" },
            { "db:conn", "Data Source=agile_config.db" },
            { "db:env:test:provider", "sqlite" },
            { "db:env:test:conn", "Data Source=agile_config1.db" }
        };
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(configMap);
        var configuration = configurationBuilder.Build();
        Global.Config = configuration;

        var myfreesql = new MyFreeSQL(new DbConfigInfoFactory(configuration));

        var fsq = myfreesql.GetInstanceByEnv("");
        Assert.IsNotNull(fsq);
        Assert.AreEqual(DataType.Sqlite, fsq.Ado.DataType);

        var fsqtest = myfreesql.GetInstanceByEnv("test");
        Assert.IsNotNull(fsqtest);
        Assert.AreEqual(DataType.Sqlite, fsqtest.Ado.DataType);

        Assert.AreNotSame(fsq, fsqtest);
        var fsqtest_ag = myfreesql.GetInstanceByEnv("test");
        Assert.AreSame(fsqtest, fsqtest_ag);


        var fsq_none = myfreesql.GetInstanceByEnv("x");
        Assert.IsNotNull(fsq_none);
        Assert.AreEqual(DataType.Sqlite, fsq_none.Ado.DataType);
        Assert.AreSame(fsq, fsq_none);
    }
}