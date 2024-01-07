using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Data.Freesql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Freesql.Tests
{
    [TestClass()]
    public class FreeSQLTests
    {
        [TestMethod()]
        public void GetInstanceByEnvTest()
        {
            var configMap = new Dictionary<string, string>() {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" },
                {"db:env:test:provider","sqlite" },
                {"db:env:test:conn","Data Source=agile_config1.db" },
            };
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(configMap);
            var configuration = configurationBuilder.Build();
            Global.Config = configuration;

            var fsq = FreeSQL.GetInstanceByEnv("");
            Assert.IsNotNull(fsq);
            Assert.AreEqual(FreeSql.DataType.Sqlite, fsq.Ado.DataType);

            var fsqtest = FreeSQL.GetInstanceByEnv("test");
            Assert.IsNotNull(fsqtest);
            Assert.AreEqual(FreeSql.DataType.Sqlite, fsqtest.Ado.DataType);

            Assert.AreNotSame(fsq, fsqtest);
            var fsqtest_ag = FreeSQL.GetInstanceByEnv("test");
            Assert.AreSame(fsqtest, fsqtest_ag);


            var fsq_none = FreeSQL.GetInstanceByEnv("x");
            Assert.IsNotNull(fsq_none);
            Assert.AreEqual(FreeSql.DataType.Sqlite, fsq_none.Ado.DataType);
            Assert.AreSame(fsq, fsq_none);
        }
    }
}