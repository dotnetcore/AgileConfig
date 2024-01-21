using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    public class BasicTestService
    {
        protected ServiceProvider _serviceProvider;

        public virtual Dictionary<string, string> GetConfigurationData()
        {
            return
                new Dictionary<string, string>
                {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" }
            };
        }

        public virtual void ClearData()
        {
            var factory = new EnvFreeSqlFactory();
            var fsq = factory.Create("");

            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishDetail>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishTimeline>().Where("1=1").ExecuteAffrows();
            fsq.Delete<ConfigPublished>().Where("1=1").ExecuteAffrows();
            //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();
        }

        public BasicTestService()
        {
            var config = new ConfigurationBuilder()
             .AddInMemoryCollection(GetConfigurationData())
             .Build();
            Global.Config = config;

            var cache = new Mock<IMemoryCache>();
            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => cache.Object);
            services.AddSingleton<IConfiguration>(config);
            services.AddFreeSqlFactory();
            services.AddRepositories();
            services.AddBusinessServices();

            _serviceProvider = services.BuildServiceProvider();
            var systeminitializationService = _serviceProvider.GetService<ISystemInitializationService>();
            systeminitializationService.TryInitDefaultEnvironmentAsync();//初始化环境 DEV TEST STAGE PROD
            systeminitializationService.TryInitJwtSecret();//初始化 jwt secret

            Console.WriteLine("Run BasicTestService");
        }
    }
}
