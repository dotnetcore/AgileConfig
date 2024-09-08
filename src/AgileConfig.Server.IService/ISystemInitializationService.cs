using System.Threading.Tasks;

namespace AgileConfig.Server.IService;

public interface ISystemInitializationService
{
    bool TryInitJwtSecret();

    bool TryInitDefaultEnvironment();
}