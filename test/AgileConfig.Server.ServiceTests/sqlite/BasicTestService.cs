using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
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
        protected IFreeSql _fsq = null;

        public virtual Dictionary<string, string> GetConfigurationData()
        {
            return
                new Dictionary<string, string>
                {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" }
            };
        }

        public BasicTestService()
        {
            var config = new ConfigurationBuilder()
             .AddInMemoryCollection(GetConfigurationData())
             .Build();
            Global.Config = config;

            var factory = new EnvFreeSqlFactory();
            _fsq = factory.Create("");

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

        private static void AddEnvRepositiroies(IServiceCollection sc)
        {
            sc.AddScoped<Func<string, IUow>>(sp => env =>
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                var fsq = factory.Create(env);
                return new FreeSqlUow(fsq);
            });

            sc.AddScoped<Func<string, IConfigPublishedRepository>>(sp => env =>
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new ConfigPublishedRepository(factory.Create(env));
            });

            sc.AddScoped<Func<string, IConfigRepository>>(sp => env =>
            {
                var factory = sp.GetService<IFreeSqlFactory>();

                return new ConfigRepository(factory.Create(env));
            });

            sc.AddScoped<Func<string, IPublishDetailRepository>>(sp => env =>
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new PublishDetailRepository(factory.Create(env));
            });

            sc.AddScoped<Func<string, IPublishTimelineRepository>>(sp => env =>
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new PublishTimelineRepository(factory.Create(env));
            });
        }

    }
}
