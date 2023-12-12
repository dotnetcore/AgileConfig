using AgileConfig.Server.Data.Mongodb;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Mongodb.Service;

public static class MongodbServiceExtensions
{
    public static void AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }
}