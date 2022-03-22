using AgileConfig.Server.IService;
using AgileConfig.Server.Service.EventRegisterService;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Service
{
    public static class ServiceCollectionExt
    {
        public static void AddBusinessServices(this IServiceCollection sc)
        {
            sc.AddSingleton<IRemoteServerNodeProxy, RemoteServerNodeProxy>();
            sc.AddSingleton<IEventRegister, EventRegister>();
            sc.AddSingleton<IServiceHealthCheckService, ServiceHealthCheckService>();
            sc.AddSingleton<IServiceInfoService, ServiceInfoService>();
            
            sc.AddScoped<IAppService, AppService>();
            sc.AddScoped<IConfigService, ConfigService>();
            sc.AddScoped<IServerNodeService, ServerNodeService>();
            sc.AddScoped<ISettingService, SettingService>();
            sc.AddScoped<ISysLogService, SysLogService>();
            sc.AddScoped<IAppBasicAuthService, AppBasicAuthService>();
            sc.AddScoped<IAdmBasicAuthService, AdmBasicAuthService>();
            sc.AddScoped<IUserService, UserService>();
            sc.AddScoped<IPremissionService, PermissionService>();
            sc.AddScoped<IRegisterCenterService, RegisterCenterService>();
        }
    }
}
