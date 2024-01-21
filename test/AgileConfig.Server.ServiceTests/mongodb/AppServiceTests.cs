using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using AgileConfig.Server.Data.Repository.Mongodb;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    [TestClass()]
    public class AppServiceTests_mongo : AppServiceTests
    {
        public override void ClearData()
        {
            var repository = new AppRepository(conn);
            var entities = repository.AllAsync().Result;
            foreach (var entity in entities)
            {
                repository.DeleteAsync(entity).Wait();
            }
        }

        string conn = "mongodb://192.168.0.125:27017/agile_config_1";

        public override Dictionary<string, string> GetConfigurationData()
        {
            var dict = base.GetConfigurationData();
            dict["db:provider"] = "mongodb";
            dict["db:conn"] = conn;

            return dict;
        }
    }
}