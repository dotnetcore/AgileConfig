using AgileConfig.Server.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Repository
{
    public static class ServiceCollectionExt
    {
        public static void AddAgileConfigDb(this IServiceCollection sc)
        {
            sc.AddScoped<ISqlContext, AgileConfigDbContext>();
            using (var ctx = new AgileConfigDbContext())
            {
                ctx.InitTables();
            }
        }
    }
}
