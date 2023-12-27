using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction;

public interface ISysInitRepository
{
    /// <summary>
    /// get jwt token secret from db
    /// </summary>
    /// <returns></returns>
    string? GetJwtTokenSecret();

    /// <summary>
    /// save initialization setting 
    /// </summary>
    /// <param name="setting"></param>
    void SaveInitSetting(Setting setting);
}