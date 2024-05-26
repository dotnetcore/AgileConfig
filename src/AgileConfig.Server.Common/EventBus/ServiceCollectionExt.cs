using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Common.EventBus
{
    public static class ServiceCollectionExt
    {
        public static IServiceCollection AddTinyEventBus(this IServiceCollection sc)
        {
            sc.AddSingleton<ITinyEventBus, TinyEventBus>(sp => new TinyEventBus(sc));
            return sc;
        }
    }
}
