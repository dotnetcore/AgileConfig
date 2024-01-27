using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.Data.Repository.Mongodb;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Selector
{
    public static class RepositoryExtension
    {
        public static IServiceCollection AddRepositories(this IServiceCollection sc)
        {
            sc.AddFreeRepository();

            var defaultProvider = DbConfigInfoFactory.GetConfigInfo();

            if (string.IsNullOrEmpty(defaultProvider.Provider))
            {
                throw new ArgumentNullException(nameof(defaultProvider));
            }

            Console.WriteLine($"default db provider: {defaultProvider.Provider}");

            #region add default fixed repositories

            GetRepositoryServiceRegister(defaultProvider.Provider).AddFixedRepositories(sc);
          
            #endregion

            #region these repositories genereated dependency env provider, if no env provider use default provider
            sc.AddScoped<Func<string, IUow>>(sp => env =>
            {
                var envDbConfig = DbConfigInfoFactory.GetConfigInfo(env);

                return GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IUow>(sp, env);
            });

            sc.AddScoped<Func<string, IConfigPublishedRepository>>(sp => env =>
            {
                var envDbConfig = DbConfigInfoFactory.GetConfigInfo(env);

                return GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IConfigPublishedRepository>(sp, env);
            });

            sc.AddScoped<Func<string, IConfigRepository>>(sp => env =>
            {
                var envDbConfig = DbConfigInfoFactory.GetConfigInfo(env);

                return GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IConfigRepository>(sp, env);
            });

            sc.AddScoped<Func<string, IPublishDetailRepository>>(sp => env =>
            {
                var envDbConfig = DbConfigInfoFactory.GetConfigInfo(env);

                return GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IPublishDetailRepository>(sp, env);
            });

            sc.AddScoped<Func<string, IPublishTimelineRepository>>(sp => env =>
            {
                var envDbConfig = DbConfigInfoFactory.GetConfigInfo(env);

                return GetRepositoryServiceRegister(envDbConfig.Provider).GetServiceByEnv<IPublishTimelineRepository>(sp, env);
            });
            #endregion

            return sc;
        }

        // if add new type of repository service, add it here
        private static List<IRepositoryServiceRegister> _repositoryServiceRegisters = new List<IRepositoryServiceRegister>() {
            new FreesqlRepositoryServiceRegister(),
            new MongodbRepositoryServiceRegister()
        };

        private static IRepositoryServiceRegister GetRepositoryServiceRegister(string provider)
        {
            foreach (var register in _repositoryServiceRegisters)
            {
                if (register.IsSuit4Provider(provider))
                {
                    return register;
                }
            }

            throw new ArgumentException($"[{provider}] is not a supported provider.");
        }
    }
}