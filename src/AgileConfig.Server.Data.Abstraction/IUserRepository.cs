using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IUserRepository : IRepository<User, string>
    {
    }
}
