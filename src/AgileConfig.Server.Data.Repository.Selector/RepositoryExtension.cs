using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.Data.Repository.Mongodb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AgileConfig.Server.Data.Repository.Selector;

public static class RepositoryExtension
{
    // if add new type of repository service, add it here
    private static readonly List<IRepositoryServiceRegister> _repositoryServiceRegisters = new()
    {
        new FreesqlRepositoryServiceRegister(),
        new MongodbRepositoryServiceRegister()
    };

    public static IServiceCollection AddRepositories(this IServiceCollection sc)
    {
        sc.AddFreeRepository();
        sc.TryAddSingleton(TimeProvider.System);
        sc.AddSingleton<DatabaseCircuitRegistry>();
        sc.AddSingleton(sp => sp.GetRequiredService<DatabaseCircuitRegistry>().Get());

        var serviceProvider = sc.BuildServiceProvider();
        var dbConfigInfoFactory = serviceProvider.GetRequiredService<IDbConfigInfoFactory>();

        var defaultProvider = dbConfigInfoFactory.GetConfigInfo();

        if (string.IsNullOrEmpty(defaultProvider.Provider)) throw new ArgumentNullException(nameof(defaultProvider));

        Console.WriteLine($"default db provider: {defaultProvider.Provider}");

        #region add default fixed repositories

        var firstRepository = sc.Count;
        GetRepositoryServiceRegister(defaultProvider.Provider).AddFixedRepositories(sc);
        for (var i = firstRepository; i < sc.Count; i++)
        {
            var descriptor = sc[i];
            sc[i] = ServiceDescriptor.Describe(descriptor.ServiceType, sp =>
                ResilientRepositoryProxy.Wrap(descriptor.ServiceType,
                    () => ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType),
                    sp.GetRequiredService<DatabaseCircuitRegistry>().Get()), descriptor.Lifetime);
        }

        #endregion

        #region these repositories genereated dependency env provider, if no env provider use default provider

        sc.AddScoped<Func<string, IUow>>(sp => env =>
        {
            var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
            return sp.GetRequiredService<DatabaseCircuitRegistry>().Get(env).CreateResource(() =>
                GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IUow>(sp, env));
        });

        sc.AddScoped<Func<string, IConfigPublishedRepository>>(sp => env =>
        {
            var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);

            return Wrap<IConfigPublishedRepository>(sp, env, envDbConfig.Provider);
        });

        sc.AddScoped<Func<string, IConfigRepository>>(sp => env =>
        {
            var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);

            return Wrap<IConfigRepository>(sp, env, envDbConfig.Provider);
        });

        sc.AddScoped<Func<string, IPublishDetailRepository>>(sp => env =>
        {
            var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);

            return Wrap<IPublishDetailRepository>(sp, env, envDbConfig.Provider);
        });

        sc.AddScoped<Func<string, IPublishTimelineRepository>>(sp => env =>
        {
            var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);

            return Wrap<IPublishTimelineRepository>(sp, env, envDbConfig.Provider);
        });

        #endregion

        return sc;
    }

    private static T Wrap<T>(IServiceProvider sp, string env, string provider) where T : class
    {
        return (T)ResilientRepositoryProxy.Wrap(typeof(T),
            () => GetRepositoryServiceRegister(provider).GetServiceByEnv<T>(sp, env),
            sp.GetRequiredService<DatabaseCircuitRegistry>().Get(env));
    }

    private static IRepositoryServiceRegister GetRepositoryServiceRegister(string provider)
    {
        foreach (var register in _repositoryServiceRegisters)
            if (register.IsSuit4Provider(provider))
                return register;

        throw new ArgumentException($"[{provider}] is not a supported provider.");
    }
}