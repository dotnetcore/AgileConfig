using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public static class ServiceCollectionExt
    {
        public static void AddFreeSqlDbContext(this IServiceCollection sc)
        {
            sc.AddFreeDbContext<FreeSqlContext>(options => options.UseFreeSql(FreeSQL.Instance));
        }
    }
}
