using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public static class ServiceCollectionExt
    {
        public static void AddFreeSqlFactory(this IServiceCollection sc)
        {
            sc.AddKeyedSingleton<IFreeSqlFactory, DefaultFreeSqlFactory>("");
            sc.AddKeyedSingleton<IFreeSqlFactory, EnvFreeSqlFactory>("Env");
        }
    }
}
