using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class SettingRepository : FreesqlRepository<Setting, string>, ISettingRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public SettingRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
    {
        this.freeSqlFactory = freeSqlFactory;
    }
}