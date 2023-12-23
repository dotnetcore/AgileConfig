using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class UserRoleRepository : FreesqlRepository<UserRole, string>, IUserRoleRepository
    {
        public UserRoleRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
