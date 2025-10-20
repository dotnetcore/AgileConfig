using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;

namespace AgileConfig.Server.Data.Repository.Mongodb
{
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
            sc.AddSingleton<ISysInitRepository, SysInitRepository>();
        }

        public T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class
        {
            var dbConfigInfoFactory = sp.GetRequiredService<IDbConfigInfoFactory>();

            if (typeof(T) == typeof(IUow))
            {
                return new MongodbUow() as T;
            }
            if (typeof(T) == typeof(IConfigPublishedRepository))
            {
                var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
                return new ConfigPublishedRepository(envDbConfig.ConnectionString) as T;
            }
            if (typeof(T) == typeof(IConfigRepository))
            {
                var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
                return new ConfigRepository(envDbConfig.ConnectionString) as T;
            }
            if (typeof(T) == typeof(IPublishDetailRepository))
            {
                var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
                return new PublishDetailRepository(envDbConfig.ConnectionString) as T;
            }
            if (typeof(T) == typeof(IPublishTimelineRepository))
            {
                var envDbConfig = dbConfigInfoFactory.GetConfigInfo(env);
                return new PublishTimelineRepository(envDbConfig.ConnectionString) as T;
            }

            return default(T);
        }

        public bool IsSuit4Provider(string provider)
        {
            switch (provider.ToLower())
            {
                case "mongodb":
                    return true;
                default:
                    break;
            }

            return false;
        }
    }
}
