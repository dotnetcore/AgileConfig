using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class UserRoleRepository : FreesqlRepository<UserRole, string>, IUserRoleRepository
    {
        private readonly IFreeSql freeSql;

        public UserRoleRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}
