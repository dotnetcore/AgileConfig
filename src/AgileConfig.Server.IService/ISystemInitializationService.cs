namespace AgileConfig.Server.IService;

public interface ISystemInitializationService
{
    bool TryInitJwtSecret();
}