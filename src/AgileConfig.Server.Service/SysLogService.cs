using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

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

        public Task<long> Count(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime)
        {
            Expression<Func<SysLog, bool>> exp = x => true;
            if (!string.IsNullOrEmpty(appId))
            {
                exp = exp.And(c => c.AppId == appId);
            }

            if (startTime.HasValue)
            {
                exp = exp.And(x => x.LogTime >= startTime);
            }

            if (endTime.HasValue)
            {
                exp = exp.And(x => x.LogTime < endTime);
            }

            if (logType.HasValue)
            {
                exp = exp.And(x => x.LogType == logType);
            }

            return _sysLogRepository.CountAsync(exp);
        }

        public void Dispose()
        {
            _sysLogRepository.Dispose();
        }

        public Task<List<SysLog>> SearchPage(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime, int pageSize, int pageIndex)
        {
            Expression<Func<SysLog, bool>> exp = x => true;
            if (!string.IsNullOrEmpty(appId))
            {
                exp = exp.And(c => c.AppId == appId);
            }

            if (startTime.HasValue)
            {
                exp = exp.And(x => x.LogTime >= startTime);
            }

            if (endTime.HasValue)
            {
                exp = exp.And(x => x.LogTime < endTime);
            }

            if (logType.HasValue)
            {
                exp = exp.And(x => x.LogType == logType);
            }

            return _sysLogRepository.QueryPageAsync(exp, pageIndex, pageSize, defaultSortField: "LogTime", defaultSortType: "DESC");
        }
    }
}
