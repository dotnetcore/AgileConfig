using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class UserRoleRepository : FreesqlRepository<UserRole, string>, IUserRoleRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public UserRoleRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
    {
        this.freeSqlFactory = freeSqlFactory;
    }
}