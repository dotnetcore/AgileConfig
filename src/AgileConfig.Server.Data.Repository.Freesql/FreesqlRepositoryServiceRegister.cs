using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class FreesqlRepositoryServiceRegister : IRepositoryServiceRegister
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
            var factory = sp.GetService<IFreeSqlFactory>();

            if (typeof(T) == typeof(IUow))
            {
                return (new FreeSqlUow(factory.Create(env))) as T;
            }
            if (typeof(T) == typeof(IConfigPublishedRepository))
            {
                return new ConfigPublishedRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IConfigRepository))
            {
                return new ConfigRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IPublishDetailRepository))
            {
                return new PublishDetailRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IPublishTimelineRepository))
            {
                return new PublishTimelineRepository(factory.Create(env)) as T;
            }

            return default(T);
        }

        public bool IsSuit4Provider(string provider)
        {
            var freesqlType = MyFreeSQL.ProviderToFreesqlDbType(provider);

            return freesqlType.HasValue ;
        }
    }
}
