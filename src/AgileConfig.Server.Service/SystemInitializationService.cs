using System;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class SystemInitializationService(
    ISysInitRepository sysInitRepository,
    IConfiguration configuration,
    ILogger<SystemInitializationService> logger) : ISystemInitializationService
{
    /// <summary>
    /// Initialize the JWT secret if it is not configured via file or environment variables.
    /// </summary>
    /// <returns></returns>
    public bool TryInitJwtSecret()
    {
        var jwtSecretFromConfig = configuration["JwtSetting:SecurityKey"];
        if (string.IsNullOrEmpty(jwtSecretFromConfig))
        {
            var jwtSecretSetting = sysInitRepository.GetJwtTokenSecret();
            if (jwtSecretSetting == null)
            {
                var setting = new Setting
                {
                    Id = SystemSettings.DefaultJwtSecretKey,
                    Value = GenerateJwtSecretKey(),
                    CreateTime = DateTime.Now
                };

                try
                {
                    sysInitRepository.SaveInitSetting(setting);
                    return true;
                }
                catch (Exception e)
                {
                    // Handle concurrent initialization across multiple instances.
                    Console.WriteLine(e);
                }

                return false;
            }
        }

        return true;
    }

    public bool TryInitDefaultEnvironment()
    {
        var envArrayString = sysInitRepository.GetDefaultEnvironmentFromDb();
        if (envArrayString == null)
        {
            envArrayString = SystemSettings.DefaultEnvironment;
            var setting = new Setting
            {
                Id = SystemSettings.DefaultEnvironmentKey,
                Value = envArrayString,
                CreateTime = DateTime.Now
            };
            try
            {
                sysInitRepository.SaveInitSetting(setting);
            }
            catch (Exception e)
            {
                logger.LogError("TryInitDefaultEnvironment error, maybe exec this saveing action in parallel on another node.");
            }
        }

        ISettingService.EnvironmentList = envArrayString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        return true;
    }

    /// <summary>
    /// Generate a 38-character JWT secret key.
    /// </summary>
    /// <returns></returns>
    private static string GenerateJwtSecretKey()
    {
        var guid1 = Guid.NewGuid().ToString("n");
        var guid2 = Guid.NewGuid().ToString("n");

        return guid1[..19] + guid2[..19];
    }

    /// <summary>
    /// Initialize the super administrator password, optionally reading from configuration when no value is provided.
    /// </summary>
    /// <param name="password">Plain text password to set for the super administrator, or empty to read from configuration.</param>
    /// <returns>True if the password is already set or initialization completed successfully; otherwise false.</returns>
    public bool TryInitSaPassword(string password = "")
    {
        if (string.IsNullOrEmpty(password))
        {
            password = configuration["saPassword"];
        }
        if (!string.IsNullOrEmpty(password) && !sysInitRepository.HasSa())
        {
            try
            {
                sysInitRepository.InitSa(password);
                logger.LogInformation("Init super admin password successful.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Init super admin password occur error.");
                return false;
            }
        }

        return true;
    }

    public bool HasSa()
    {
        return sysInitRepository.HasSa();
    }

    public bool TryInitDefaultApp(string appName = "")
    {
        if (string.IsNullOrEmpty(appName))
        {
            appName = configuration["defaultApp"];
        }

        if (!string.IsNullOrEmpty(appName))
        {
            try
            {
                sysInitRepository.InitDefaultApp(appName);
                logger.LogInformation("Init default app {appName} successful.", appName);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Init default app {appName} error.", appName);
                return false;
            }
        }

        return true;
    }
}