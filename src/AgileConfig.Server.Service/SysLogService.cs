using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.Logging;
using static FreeSql.Internal.GlobalFilter;

namespace AgileConfig.Server.Service
{
    public class SysLogService : ISysLogService
    {
        private readonly ISysLogRepository _sysLogRepository;
        private readonly ILogger<SysLogService> _logger;

        public SysLogService(ISysLogRepository sysLogRepository, ILogger<SysLogService> logger)
        {
            _sysLogRepository = sysLogRepository;
            _logger = logger;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<SysLog> logs)
        {
            await _sysLogRepository.InsertAsync(logs.ToList());

            foreach (var item in logs)
            {
                _logger.LogInformation("{AppId} {LogType} {LogTime} {LogText}", item.AppId, item.LogType, item.LogTime, item.LogText);
            }

            return true;
        }

        public async Task<bool> AddSysLogAsync(SysLog log)
        {
            await _sysLogRepository.InsertAsync(log);

            _logger.LogInformation("{AppId} {LogType} {LogTime} {LogText}", log.AppId, log.LogType, log.LogTime, log.LogText);

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
