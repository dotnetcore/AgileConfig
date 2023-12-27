using System;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Service;

public class SystemInitializationService(ISysInitRepository sysInitRepository):ISystemInitializationService
{
    /// <summary>
    /// 如果 配置文件或者环境变量没配置 JwtSetting:SecurityKey 则生成一个存库
    /// </summary>
    /// <returns></returns>
    public bool TryInitJwtSecret()
    {
        var jwtSecretFromConfig = Global.Config["JwtSetting:SecurityKey"];
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