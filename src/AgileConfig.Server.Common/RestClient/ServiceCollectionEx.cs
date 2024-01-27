using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Common.RestClient
{
    public static class ServiceCollectionEx
    {
        public static void AddRestClient(this IServiceCollection sc)
        {
            sc.AddScoped<IRestClient, DefaultRestClient>();
        }
    }
}
