using AgileConfig.Server.Data.Abstraction.DbProvider;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Data.Abstraction
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDbConfigInfoFactory(this IServiceCollection sc)
        {
            sc.AddSingleton<IDbConfigInfoFactory, DbConfigInfoFactory>();

            return sc;
        }
    }
}
