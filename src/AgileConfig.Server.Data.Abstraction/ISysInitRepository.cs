﻿using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Abstraction;

public interface ISysInitRepository
{
    /// <summary>
    /// get jwt token secret from db
    /// </summary>
    /// <returns></returns>
    string? GetJwtTokenSecret();

    /// <summary>
    /// get default environment from db
    /// </summary>
    /// <returns></returns>
    string? GetDefaultEnvironmentFromDb();

    /// <summary>
    /// save initialization setting
    /// </summary>
    /// <param name="setting">Setting values to persist.</param>
    void SaveInitSetting(Setting setting);

    /// <summary>
    /// Init super admin
    /// </summary>
    /// <param name="password">Password to assign to the super administrator.</param>
    /// <returns></returns>
    bool InitSa(string password);

    bool HasSa();

    bool InitDefaultApp(string appName);
}