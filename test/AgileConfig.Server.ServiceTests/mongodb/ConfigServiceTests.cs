using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.ServiceTests.sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.mongodb
{
    public class ConfigServiceTests_mongo : ConfigServiceTests
    {
        public override void ClearData()
        {
            var repository = new ConfigRepository(conn);
            var entities = repository.AllAsync().Result;
            foreach (var entity in entities)
            {
                repository.DeleteAsync(entity).Wait();
            }

            var configPublishedRepository = new ConfigPublishedRepository(conn);
            var configPublisheds = configPublishedRepository.AllAsync().Result;
            foreach (var entity in configPublisheds)
            {
                configPublishedRepository.DeleteAsync(entity).Wait();
            }

            var detailRepository = new PublishDetailRepository(conn);
            var details = detailRepository.AllAsync().Result;
            foreach (var entity in details)
            {
                detailRepository.DeleteAsync(entity).Wait();
            }

            var app_repository = new AppRepository(conn);
            var apps = app_repository.AllAsync().Result;
            foreach (var entity in apps)
            {
                app_repository.DeleteAsync(entity).Wait();
            }

            var appref_repository = new AppInheritancedRepository(conn);
            var apprefs = appref_repository.AllAsync().Result;
            foreach (var entity in apprefs)
            {
                appref_repository.DeleteAsync(entity).Wait();
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