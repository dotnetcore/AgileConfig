using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileConfig.Server.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgileConfig.Server.Service
{
    public class SysLogService : ISysLogService
    {
        private AgileConfigDbContext _dbContext;

        public SysLogService(ISqlContext context)
        {
            _dbContext = context as AgileConfigDbContext;
        }

        public async Task<bool> AddRangeAsync(List<SysLog> logs)
        {
            await _dbContext.SysLogs.AddRangeAsync(logs);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<bool> AddSysLogAsync(SysLog log)
        {
            await _dbContext.SysLogs.AddAsync(log);
            int x = await _dbContext.SaveChangesAsync();
            return x > 0;
        }

        public async Task<long> Count(string appId, DateTime startTime, DateTime endTime)
        {
            var query = _dbContext.SysLogs.AsNoTracking();
            if (!string.IsNullOrEmpty(appId))
            {
                query = query.Where(x => x.AppId == appId);
            }
            query = query.Where(x => x.LogTime >= startTime);
            query = query.Where(x => x.LogTime < endTime);


            var count = await query.LongCountAsync();

            return count;
        }

        public Task<List<SysLog>> SearchPage(string appId, DateTime startTime, DateTime endTime, int pageSize, int pageIndex)
        {
            var query = _dbContext.SysLogs.AsNoTracking();
            if (!string.IsNullOrEmpty(appId))
            {
                query = query.Where(x => x.AppId == appId);
            }
            query = query.Where(x => x.LogTime >= startTime);
            query = query.Where(x => x.LogTime < endTime);

            query = query.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return query.ToListAsync();
        }
    }
}
