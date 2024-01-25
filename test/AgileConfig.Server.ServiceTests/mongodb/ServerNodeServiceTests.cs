using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using AgileConfig.Server.ServiceTests.sqlite;
using AgileConfig.Server.Data.Repository.Mongodb;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    public class ServerNodeServiceTests_mongo: ServerNodeServiceTests
    {
        public override void ClearData()
        {
            var repository = new ServerNodeRepository(conn);
            var entities = repository.AllAsync().Result;
            foreach (var log in entities)
            {
                repository.DeleteAsync(log).Wait();
            }
        }

        string conn = "mongodb://192.168.0.125:27017/agile_config_1";

        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","mongodb" },
                {"db:conn",conn }
            });
        }
    }
}