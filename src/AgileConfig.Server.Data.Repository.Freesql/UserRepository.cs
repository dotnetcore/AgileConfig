using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class UserRepository : FreesqlRepository<User, string>, IUserRepository
    {
        public UserRepository([FromKeyedServices("")] IFreeSqlFactory freeSql) : base(freeSql)
        {
        }
    }
}
