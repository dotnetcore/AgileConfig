using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Freesql
{
    public static class ServiceCollectionExt
    {
        public static void AddFreeSqlFactory(this IServiceCollection sc)
        {
            sc.AddScoped<IUow, FreeSqlUow>();
            sc.AddSingleton<IFreeSqlFactory, EnvFreeSqlFactory>();
        }
    }
}
