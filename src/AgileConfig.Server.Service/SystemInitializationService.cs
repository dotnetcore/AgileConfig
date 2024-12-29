using System;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Service;

public class SystemInitializationService(ISysInitRepository sysInitRepository, IConfiguration configuration):ISystemInitializationService
{
    /// <summary>
    /// 如果 配置文件或者环境变量没配置 JwtSetting:SecurityKey 则生成一个存库
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
                    //处理异常，防止多个实例第一次启动的时候，并发生成key值，发生异常，导致服务起不来
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
            sysInitRepository.SaveInitSetting(setting);
        }

        ISettingService.EnvironmentList = envArrayString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        return true;
    }

    /// <summary>
    /// 生成一个 jwt 加密的 key ，38位
    /// </summary>
    /// <returns></returns>
    private static string GenerateJwtSecretKey()
    {
        var guid1 = Guid.NewGuid().ToString("n");
        var guid2 =  Guid.NewGuid().ToString("n");

        return guid1[..19] + guid2[..19];
    }
}