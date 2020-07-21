using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ISysLogService
    {
        Task<bool> AddSysLogAsync(SysLog log);

        Task<bool> AddRangeAsync(List<SysLog> logs);


        Task<List<SysLog>> SearchPage(string appId, DateTime startTime, DateTime endTime, int pageSize, int pageIndex);

        Task<long> Count(string appId, DateTime startTime, DateTime endTime);

    }
}
