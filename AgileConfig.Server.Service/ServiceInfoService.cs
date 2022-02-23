using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service
{
    public class ServiceInfoService : IServiceInfoService
    {
        private FreeSqlContext _dbContext;

        public ServiceInfoService(FreeSqlContext freeSql)
        {
            _dbContext = freeSql;
        }

        public async Task<List<ServiceInfo>> GetAllServiceInfoAsync()
        {
            var services = await _dbContext.ServiceInfo.Where(x => 1 == 1).ToListAsync();

            return services;
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}