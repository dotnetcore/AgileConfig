using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IUserAppAuthRepository : IRepository<UserAppAuth, string>
    {
    }
}
