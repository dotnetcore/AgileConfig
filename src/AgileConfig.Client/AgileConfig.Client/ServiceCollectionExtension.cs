using AgileConfig.Client;
using AgileConfig.Client.RegisterCenter;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static void AddAgileConfig(this IServiceCollection services)
        {
            ConfigClient.Instance.Logger = GetLogger(services);
            services.AddSingleton<IConfigClient>(sp =>
            {
                ConfigClient.Instance.Logger = GetLogger(services);
                return ConfigClient.Instance;
            });
            if (ConfigClient.Instance.Options.RegisterInfo != null)
            {
                AddAgileRegisterCenterDiscovery(services);
            }
        }

        public static void AddAgileRegisterCenterDiscovery(this IServiceCollection services)
        {
            services.AddSingleton<IRegisterService, RegisterService>();
            services.AddSingleton<IDiscoveryService, DiscoveryService>();
            services.AddHostedService<RegisterHostedService>();
        }

        private static ILogger GetLogger(IServiceCollection services)
        {
            var logger = services.BuildServiceProvider().GetService<ILoggerFactory>().CreateLogger<ConfigClient>();
            return logger;
        }
    }
}
