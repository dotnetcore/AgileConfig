using System.Threading.Tasks;

namespace AgileConfig.Server.IService;

public interface ISystemInitializationService
{
    bool TryInitDefaultApp(string appName = "");

    bool TryInitSaPassword(string password = "");

    bool HasSa();

    bool TryInitJwtSecret();

    bool TryInitDefaultEnvironment();
}