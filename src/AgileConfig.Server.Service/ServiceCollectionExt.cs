#nullable enable
using AgileConfig.Server.IService;
using AgileConfig.Server.Service.EventRegisterService;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Service
{
    public static class ServiceCollectionExt
    {
        public static void AddBusinessServices(this IServiceCollection sc)
        {
            sc.AddSingleton<ISystemInitializationService, SystemInitializationService>();
            sc.AddSingleton<IJwtService, JwtService>();

            sc.AddScoped<IRemoteServerNodeProxy, RemoteServerNodeProxy>();
            sc.AddScoped<IEventHandlerRegister, EventHandlerRegister>();
            sc.AddScoped<IServiceHealthCheckService, ServiceHealthCheckService>();
            sc.AddScoped<IServiceInfoService, ServiceInfoService>();

            sc.AddScoped<IAppService, AppService>();
            sc.AddScoped<IConfigService, ConfigService>();
            sc.AddScoped<IServerNodeService, ServerNodeService>();
            sc.AddScoped<ISettingService, SettingService>();
            sc.AddScoped<ISysLogService, SysLogService>();
            sc.AddScoped<IAppBasicAuthService, AppBasicAuthService>();
            sc.AddScoped<IAdmBasicAuthService, AdmBasicAuthService>();
            sc.AddScoped<IUserService, UserService>();
            sc.AddScoped<IPermissionService, PermissionService>();
            sc.AddScoped<IRoleService, RoleService>();
            sc.AddScoped<IRegisterCenterService, RegisterCenterService>();
            
            sc.AddScoped<ConfigStatusUpdateEventHandlersRegister>();
            sc.AddScoped<ServiceInfoStatusUpdateEventHandlersRegister>();
            sc.AddScoped<SystemEventHandlersRegister>();

        }
    }
}
