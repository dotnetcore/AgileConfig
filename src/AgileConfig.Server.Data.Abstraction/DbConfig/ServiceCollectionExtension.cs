using AgileConfig.Server.Data.Abstraction.DbProvider;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Abstraction;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDbConfigInfoFactory(this IServiceCollection sc)
    {
        sc.AddSingleton<IDbConfigInfoFactory, DbConfigInfoFactory>();

        return sc;
    }
}