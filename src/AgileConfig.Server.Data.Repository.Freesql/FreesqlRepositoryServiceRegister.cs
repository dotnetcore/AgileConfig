﻿using AgileConfig.Server.Data.Abstraction;
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
            sc.AddSingleton<ISysInitRepository, SysInitRepository>();
        }

        public T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class
        {
            if (typeof(T) == typeof(IUow))
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                var fsq = factory.Create(env);
                return (new FreeSqlUow(fsq)) as T;
            }
            if (typeof(T) == typeof(IConfigPublishedRepository))
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new ConfigPublishedRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IConfigRepository))
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new ConfigRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IPublishDetailRepository))
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new PublishDetailRepository(factory.Create(env)) as T;
            }
            if (typeof(T) == typeof(IPublishTimelineRepository))
            {
                var factory = sp.GetService<IFreeSqlFactory>();
                return new PublishTimelineRepository(factory.Create(env)) as T;
            }

            return default(T);
        }

        public bool IsSuit4Provider(string provider)
        {
            var freesqlType = MyFreeSQL.ProviderToFreesqlDbType(provider);

            return freesqlType.HasValue;
        }
    }
}
