using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Freesql
{
    public static class ServiceCollectionExt
    {
        public static void AddFreeSqlFactory(this IServiceCollection sc)
        {
            sc.AddSingleton<IFreeSql>(FreeSQL.Instance);
            sc.AddSingleton<IFreeSqlFactory, EnvFreeSqlFactory>();
        }
    }
}
