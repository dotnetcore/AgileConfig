using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class SysLogRepository : FreesqlRepository<SysLog, string>, ISysLogRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public SysLogRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
    {
        this.freeSqlFactory = freeSqlFactory;
    }
}