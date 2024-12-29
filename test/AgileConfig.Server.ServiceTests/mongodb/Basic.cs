using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AgileConfig.Server.ServiceTests.mongodb;

public abstract class Basic
{
    private static readonly Dictionary<string, string> ConfigurationData = new Dictionary<string, string>
    {
        {
            "db:provider",
            "mongodb"
        },
        {
            "db:conn",
            "mongodb://localhost:27017;localhost:37017/AgileConfig"
        }
    };
    private static void InitServices(IServiceCollection sc, string connectionString)
    {
        sc.AddTransient<Func<string, IUow>>(_ => _ => new MongodbUow());

        sc.AddScoped<Func<string, IConfigPublishedRepository>>(
            _ => _ => new ConfigPublishedRepository(connectionString));

        sc.AddScoped<Func<string, IConfigRepository>>(_ => _ => new ConfigRepository(connectionString));

        sc.AddScoped<Func<string, IPublishDetailRepository>>(_ => _ => new PublishDetailRepository(connectionString));

        sc.AddScoped<Func<string, IPublishTimelineRepository>>(
            _ => _ => new PublishTimelineRepository(connectionString));
    }

    private readonly ServiceProvider _serviceProvider;

    protected Basic()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(ConfigurationData)
            .Build();
        var cache = new Mock<IMemoryCache>();
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<IMemoryCache>(_ => cache.Object);
        services.AddSingleton<IConfiguration>(config);
        InitServices(services, ConfigurationData["db:conn"]);
        services.AddMongodbRepository();
        services.AddBusinessServices();
        _serviceProvider = services.BuildServiceProvider();
        Console.WriteLine("TestInitialize");
    }

    protected T GetService<T>()
    {
        return _serviceProvider.GetService<T>();
    }

    protected IConfigRepository ConfigRepository => _serviceProvider.GetService<IConfigRepository>();
    
    protected IAppRepository AppRepository => _serviceProvider.GetService<IAppRepository>();

    protected IAppInheritancedRepository AppInheritancedRepository =>
        _serviceProvider.GetService<IAppInheritancedRepository>();
    
    protected ISysLogRepository SysLogRepository =>
        _serviceProvider.GetService<ISysLogRepository>();
}