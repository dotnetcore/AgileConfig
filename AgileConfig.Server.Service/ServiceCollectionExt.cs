﻿using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Service
{
    public static class ServiceCollectionExt
    {
        public static void AddBusinessServices(this IServiceCollection sc)
        {
            sc.AddScoped<IAppService, AppService>();
            sc.AddScoped<IConfigService, ConfigService>();
            sc.AddScoped<IServerNodeService, ServerNodeService>();
            sc.AddScoped<IModifyLogService, ModifyLogService>();
            sc.AddScoped<ISettingService, SettingService>();
            sc.AddSingleton<IRemoteServerNodeProxy, RemoteServerNodeProxy>();
            sc.AddScoped<ISysLogService, SysLogService>();
            sc.AddScoped<IAppBasicAuthService, AppBasicAuthService>();
            sc.AddScoped<IAdmBasicAuthService, AdmBasicAuthService>();
            sc.AddSingleton<IEventRegister, EventRegister>();
            sc.AddScoped<IUserService, UserService>();
            sc.AddScoped<IPremissionService, PermissionService>();
        }
    }
}
