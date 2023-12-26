#nullable enable
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service.EventRegisterService;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Service
{
    public delegate T? EventRegisterTransient<out T>();
    
    public delegate IEventRegister? EventRegisterResolver(string key);
    
    public static class ServiceCollectionExt
    {
        public static void AddBusinessServices(this IServiceCollection sc)
        {
            sc.AddSingleton<IJwtService, JwtService>();

            sc.AddScoped<IRemoteServerNodeProxy, RemoteServerNodeProxy>();
            sc.AddScoped<IEventRegister, EventRegister>();
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
            sc.AddScoped<IPremissionService, PermissionService>();
            sc.AddScoped<IRegisterCenterService, RegisterCenterService>();
            
            sc.AddScoped<ConfigStatusUpdateRegister>();
            sc.AddScoped<ServiceInfoStatusUpdateRegister>();
            sc.AddScoped<SysLogRegister>();
            sc.AddScoped<EventRegisterResolver>(x => key =>
            {
                return key switch
                {
                    nameof(ConfigStatusUpdateRegister) => x.GetService<ConfigStatusUpdateRegister>(),
                    nameof(ServiceInfoStatusUpdateRegister) => x.GetService<ServiceInfoStatusUpdateRegister>(),
                    nameof(SysLogRegister) => x.GetService<SysLogRegister>(),
                    _ => null
                };
            });

            sc.AddTransient<EventRegisterTransient<IConfigService>,EventRegisterTransient<ConfigService>>();
            sc.AddTransient<EventRegisterTransient<IAppService>,EventRegisterTransient<AppService>>();
            sc.AddTransient<EventRegisterTransient<IServerNodeService>,EventRegisterTransient<ServerNodeService>>();
            sc.AddTransient<EventRegisterTransient<IServiceInfoService>,EventRegisterTransient<ServiceInfoService>>();
            sc.AddTransient<EventRegisterTransient<ISysLogService>,EventRegisterTransient<SysLogService>>();
        }
    }
}
