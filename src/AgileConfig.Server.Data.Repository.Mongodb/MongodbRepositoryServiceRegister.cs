using AgileConfig.Server.Data.Abstraction.DbProvider;

namespace AgileConfig.Server.Data.Repository.Mongodb;

public class MongodbRepositoryServiceRegister : IRepositoryServiceRegister
{
    public void AddFixedRepositories(IServiceCollection sc)
    {
        sc.AddScoped<IAppInheritancedRepository, AppInheritancedRepository>();
        sc.AddScoped<IAppRepository, AppRepository>();
        sc.AddScoped<IServerNodeRepository, ServerNodeRepository>();
        sc.AddScoped<IServiceInfoRepository, ServiceInfoRepository>();
        sc.AddScoped<ISettingRepository, SettingRepository>();
        sc.AddScoped<ISysLogRepository, SysLogRepository>();
        sc.AddScoped<IUserAppAuthRepository, UserAppAuthRepository>();
        sc.AddScoped<IUserRepository, UserRepository>();
        sc.AddScoped<IUserRoleRepository, UserRoleRepository>();
        sc.AddScoped<IRoleDefinitionRepository, RoleDefinitionRepository>();
        sc.AddScoped<IFunctionRepository, FunctionRepository>();
        sc.AddScoped<IRoleFunctionRepository, RoleFunctionRepository>();
        sc.AddSingleton<ISysInitRepository, SysInitRepository>();
    }

    public T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class
    {
        var dbConfigInfoFactory = sp.GetRequiredService<IDbConfigInfoFactory>();

        if (typeof(T) == typeof(IUow)) return new MongodbUow() as T;

        var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
        var tlsOptions = MongodbTlsOptions.FromConfiguration(sp.GetService<IConfiguration>(), env);

        if (typeof(T) == typeof(IConfigPublishedRepository))
            return new ConfigPublishedRepository(envDbConfig.ConnectionString, tlsOptions) as T;

        if (typeof(T) == typeof(IConfigRepository))
            return new ConfigRepository(envDbConfig.ConnectionString, tlsOptions) as T;

        if (typeof(T) == typeof(IPublishDetailRepository))
            return new PublishDetailRepository(envDbConfig.ConnectionString, tlsOptions) as T;

        if (typeof(T) == typeof(IPublishTimelineRepository))
            return new PublishTimelineRepository(envDbConfig.ConnectionString, tlsOptions) as T;

        return default;
    }

    public bool IsSuit4Provider(string provider)
    {
        switch (provider.ToLower())
        {
            case "mongodb":
                return true;
        }

        return false;
    }
}
