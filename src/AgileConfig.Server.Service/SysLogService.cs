using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;

namespace AgileConfig.Server.Service
{
    public class SysLogService : ISysLogService
    {
        private readonly ISysLogRepository _sysLogRepository;

        public SysLogService(ISysLogRepository sysLogRepository)
        {
            _sysLogRepository = sysLogRepository;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<SysLog> logs)
        {
            await _sysLogRepository.InsertAsync(logs.ToList());
            return true;
        }

        public async Task<bool> AddSysLogAsync(SysLog log)
        {
            await _sysLogRepository.InsertAsync(log);
            return true;
        }

        public async Task<long> Count(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime)
        {
            var query = (Expression)Expression
                .Constant(true);

            if (!string.IsNullOrEmpty(appId))
            {
                Expression<Func<SysLog, bool>> expr = x => x.AppId == appId;
                query = Expression.AndAlso(query, expr);
            }

            if (startTime.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogTime >= startTime);
                query = Expression.AndAlso(query, expr);
            }

            if (endTime.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogTime < endTime);
                query = Expression.AndAlso(query, expr);
            }

            if (logType.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogType == logType);
                query = Expression.AndAlso(query, expr);
            }

            var exp = Expression.Lambda<Func<SysLog, bool>>(query);

            var count = await _sysLogRepository.CountAsync(exp);

            return count;
        }

        public void Dispose()
        {
            _sysLogRepository.Dispose();
        }

        public async Task<List<SysLog>> SearchPage(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime, int pageSize, int pageIndex)
        {
            var query = (Expression)Expression
                .Constant(true);

            if (!string.IsNullOrEmpty(appId))
            {
                Expression<Func<SysLog, bool>> expr = x => x.AppId == appId;
                query = Expression.AndAlso(query, expr);
            }

            if (startTime.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogTime >= startTime);
                query = Expression.AndAlso(query, expr);
            }

            if (endTime.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogTime < endTime);
                query = Expression.AndAlso(query, expr);
            }

            if (logType.HasValue)
            {
                Expression<Func<SysLog, bool>> expr = (x => x.LogType == logType);
                query = Expression.AndAlso(query, expr);
            }

            var exp = Expression.Lambda<Func<SysLog, bool>>(query);

            var list = await _sysLogRepository.QueryPageAsync(exp, pageIndex, pageSize, defaultSortType: "DESC");
            return list;
        }
    }
}
