using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class UserRepository : FreesqlRepository<User, string>, IUserRepository
    {
        public UserRepository(IFreeSql freeSql) : base(freeSql)
        {
        }
    }
}
