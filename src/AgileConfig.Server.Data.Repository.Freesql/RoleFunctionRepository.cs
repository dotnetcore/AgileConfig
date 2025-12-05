using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class RoleFunctionRepository : FreesqlRepository<RoleFunction, string>, IRoleFunctionRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public RoleFunctionRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
    {
        this.freeSqlFactory = freeSqlFactory;
    }
}