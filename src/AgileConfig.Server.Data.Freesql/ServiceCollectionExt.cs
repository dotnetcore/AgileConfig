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
            sc.AddSingleton<IFreeSqlFactory, EnvFreeSqlFactory>();
        }
    }
}
