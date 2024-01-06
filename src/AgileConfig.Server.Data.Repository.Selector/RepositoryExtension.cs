﻿using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Selector
{
    public static class RepositoryExtension
    {
        public static IServiceCollection AddRepositories(this IServiceCollection sc)
        {
            sc.AddFreeRepository();

            var config = Global.Config;
            var defaultProvider = config["db:provider"];

            if (string.IsNullOrEmpty(defaultProvider))
            {
                throw new ArgumentNullException(nameof(defaultProvider));
            }

            #region these repository will use default conn provider
            if (defaultProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
            {
                sc.AddScoped<IAppInheritancedRepository, Mongodb.AppInheritancedRepository>();
                sc.AddScoped<IAppRepository, Mongodb.AppRepository>();
                sc.AddScoped<IServerNodeRepository, Mongodb.ServerNodeRepository>();
                sc.AddScoped<IServiceInfoRepository, Mongodb.ServiceInfoRepository>();
                sc.AddScoped<ISettingRepository, Mongodb.SettingRepository>();
                sc.AddScoped<ISysLogRepository, Mongodb.SysLogRepository>();
                sc.AddScoped<IUserAppAuthRepository, Mongodb.UserAppAuthRepository>();
                sc.AddScoped<IUserRepository, Mongodb.UserRepository>();
                sc.AddScoped<IUserRoleRepository, Mongodb.UserRoleRepository>();
                sc.AddSingleton<ISysInitRepository, Mongodb.SysInitRepository>();
            }
            else
            {
                sc.AddScoped<IAppInheritancedRepository, Freesql.AppInheritancedRepository>();
                sc.AddScoped<IAppRepository, Freesql.AppRepository>();
                sc.AddScoped<IServerNodeRepository, Freesql.ServerNodeRepository>();
                sc.AddScoped<IServiceInfoRepository, Freesql.ServiceInfoRepository>();
                sc.AddScoped<ISettingRepository, Freesql.SettingRepository>();
                sc.AddScoped<ISysLogRepository, Freesql.SysLogRepository>();
                sc.AddScoped<IUserAppAuthRepository, Freesql.UserAppAuthRepository>();
                sc.AddScoped<IUserRepository, Freesql.UserRepository>();
                sc.AddScoped<IUserRoleRepository, Freesql.UserRoleRepository>();
                sc.AddSingleton<ISysInitRepository, Freesql.SysInitRepository>();
            }
            #endregion

            #region these repositories genereated dependency env provider, if no env provider use default provider
            sc.AddScoped<Func<string, IUow>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                else
                {
                    var factory = sp.GetService<IFreeSqlFactory>();
                    var fsq = factory.Create(env);
                    return new FreeSqlUow(fsq);
                }
            });

            sc.AddScoped<Func<string, IConfigPublishedRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return new Mongodb.ConfigPublishedRepository(GetConnectionString(env, config));
                }
                else
                {
                    var factory = sp.GetService<IFreeSqlFactory>();
                    return new Freesql.ConfigPublishedRepository(factory.Create(env));
                }
            });

            sc.AddScoped<Func<string, IConfigRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return new Mongodb.ConfigRepository(GetConnectionString(env, config));
                }
                else
                {
                    var factory = sp.GetService<IFreeSqlFactory>();

                    return new Freesql.ConfigRepository(factory.Create(env));
                }
            });

            sc.AddScoped<Func<string, IPublishDetailRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return new Mongodb.PublishDetailRepository(GetConnectionString(env, config));
                }
                else
                {
                    var factory = sp.GetService<IFreeSqlFactory>();
                    return new Freesql.PublishDetailRepository(factory.Create(env));
                }
            });

            sc.AddScoped<Func<string, IPublishTimelineRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return new Mongodb.PublishTimelineRepository(GetConnectionString(env, config));
                }
                else
                {
                    var factory = sp.GetService<IFreeSqlFactory>();
                    return new Freesql.PublishTimelineRepository(factory.Create(env));
                }
            });
            #endregion

            return sc;
        }

        private static string GetEnvProvider(string env, IConfiguration config, string defaultProvider)
        {
            var envProviderKey = $"db:env:{env}:provider";
            var envProvider = config[envProviderKey];
            if (string.IsNullOrEmpty(envProvider))
            {
                // use default provider
                envProvider = defaultProvider;
            }

            return envProvider;
        }

        private static string? GetConnectionString(string env, IConfiguration config)
        {
            return string.IsNullOrEmpty(env) ? config["db:conn"] : config[$"db:env:{env}:conn"];
        }
    }
}