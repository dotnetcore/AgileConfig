using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Service
{
    public class SysLogService : ISysLogService
    {
        private FreeSqlContext _dbContext;

        public SysLogService(FreeSqlContext context)
        {
            _dbContext = context;
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

        public async Task<long> Count(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime)
        {
            var query = _dbContext.SysLogs.Where(s => 1 == 1);
            if (!string.IsNullOrEmpty(appId))
            {
                query = query.Where(x => x.AppId == appId);
            }
            if (startTime.HasValue)
            {
                query = query.Where(x => x.LogTime >= startTime);
            }
            if (endTime.HasValue)
            {
                query = query.Where(x => x.LogTime < endTime);
            }
            if (logType.HasValue)
            {
                query = query.Where(x => x.LogType == logType);
            }

            var count = await query.CountAsync();

            return count;
        }

        public Task<List<SysLog>> SearchPage(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime, int pageSize, int pageIndex)
        {
            var query = _dbContext.SysLogs.Where(s => 1 == 1);
            if (!string.IsNullOrEmpty(appId))
            {
                query = query.Where(x => x.AppId == appId);
            }
            if (startTime.HasValue)
            {
                query = query.Where(x => x.LogTime >= startTime);
            }
            if (endTime.HasValue)
            {
                query = query.Where(x => x.LogTime < endTime);
            }
            if (logType.HasValue)
            {
                query = query.Where(x => x.LogType == logType);
            }

            query = query.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return query.ToListAsync();
        }
    }
}
