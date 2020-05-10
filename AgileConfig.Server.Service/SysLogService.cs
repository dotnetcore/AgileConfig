using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;

namespace AgileConfig.Server.Service
{
    public class SysLogService : ISysLogService
    {
        private AgileConfigDbContext _dbContext;

        public SysLogService(ISqlContext context)
        {
            _dbContext = context as AgileConfigDbContext;
        }

        public async Task<bool> AddSysLogSync(SysLog log)
        {
            await _dbContext.SysLogs.AddAsync(log);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }
    }
}
