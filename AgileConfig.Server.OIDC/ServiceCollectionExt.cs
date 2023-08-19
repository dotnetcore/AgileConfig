using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.OIDC
{
    public static class ServiceCollectionExt
    {
        public static void AddOIDC(this IServiceCollection sc)
        {
            sc.AddSingleton<IOidcSettingProvider, ConfigfileOidcSettingProvider>();
            sc.AddSingleton<IOidcClient, OidcClient>();
        }

    }
}
