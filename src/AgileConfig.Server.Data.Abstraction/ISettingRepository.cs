using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface ISettingRepository : IRepository<Setting, string>
    {
    }
}
