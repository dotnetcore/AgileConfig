using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction;

public interface ISysLogRepository : IRepository<SysLog, string>
{
}