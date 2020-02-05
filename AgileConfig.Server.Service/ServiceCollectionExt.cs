using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Service
{
    public static class ServiceCollectionExt
    {
        public static void AddBusinessServices(this IServiceCollection sc)
        {
            sc.AddTransient<IAppService, AppService>();
            sc.AddTransient<IConfigService, ConfigService>();
        }
    }
}
