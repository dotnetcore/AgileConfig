using System.Threading.Tasks;

namespace AgileConfig.Server.Application;

public interface ICurrentUserAccessor
{
    string UserName { get; }

    Task<string> GetUserIdAsync();
}
