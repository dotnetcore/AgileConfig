using AgileConfig.Server.Data.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface ISysLogService
    {
        Task<bool> AddSysLogSync(SysLog log);
    }
}
