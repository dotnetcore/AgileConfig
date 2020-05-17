using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ISysLogService
    {
        Task<bool> AddSysLogSync(SysLog log);

        Task<List<SysLog>> SearchPage(string appId, DateTime startTime, DateTime endTime, int pageSize, int pageIndex);
    }
}
