using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

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

            sc.AddScoped<Freesql.ConfigPublishedRepository>();
            sc.AddScoped<Freesql.ConfigRepository>();
            sc.AddScoped<Freesql.PublishDetailRepository>();
            sc.AddScoped<Freesql.PublishTimelineRepository>();

            sc.AddScoped<Mongodb.ConfigPublishedRepository>();
            sc.AddScoped<Mongodb.ConfigRepository>();
            sc.AddScoped<Mongodb.PublishDetailRepository>();
            sc.AddScoped<Mongodb.PublishTimelineRepository>();

            sc.AddScoped<Func<string, IConfigPublishedRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetService<Mongodb.ConfigPublishedRepository>();
                }
                else
                {
                    var envAccssor = new ManualEnvAccessor(env);
                    var factory = new EnvFreeSqlFactory(envAccssor);
                    return new Freesql.ConfigPublishedRepository(factory);
                }
            });

            sc.AddScoped<Func<string, IConfigRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetService<Mongodb.ConfigRepository>();
                }
                else
                {
                    var envAccssor = new ManualEnvAccessor(env);
                    var factory = new EnvFreeSqlFactory(envAccssor);
                    return new Freesql.ConfigRepository(factory);
                }
            });

            sc.AddScoped<Func<string, IPublishDetailRepository>>(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetService<Mongodb.PublishDetailRepository>();
                }
                else
                {
                    var envAccssor = new ManualEnvAccessor(env);
                    var factory = new EnvFreeSqlFactory(envAccssor);
                    return new Freesql.PublishDetailRepository(factory);
                }
            });

            sc.AddScoped((Func<IServiceProvider, Func<string, IPublishTimelineRepository>>)(sp => env =>
            {
                string envProvider = GetEnvProvider(env, config, defaultProvider);

                if (envProvider.Equals("mongodb", StringComparison.OrdinalIgnoreCase))
                {
                    return sp.GetService<Mongodb.PublishTimelineRepository>();
                }
                else
                {
                    var envAccssor = new ManualEnvAccessor(env);
                    var factory = new EnvFreeSqlFactory(envAccssor);
                    return new Freesql.PublishTimelineRepository(factory);
                }
            }));

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

        class ManualEnvAccessor : IEnvAccessor
        {
            string _env;
            public ManualEnvAccessor(string env)
            {
                _env = env;
            }
            public string Env => _env;
        }
    }
}
