using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.IService;
using AgileConfig.Server.Mongodb.Service.EventRegisterService;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Mongodb.Service;

public delegate IEventRegister? EventRegisterResolver(string key);

public static class MongodbServiceExtensions
{
    public static void AddBusinessForMongoServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton<IJwtService, JwtService>();

        services.AddScoped<IRemoteServerNodeProxy, RemoteServerNodeProxy>();
        
        services.AddScoped<IEventRegister, EventRegister>();
        services.AddScoped<ConfigStatusUpdateRegister>();
        services.AddScoped<ServiceInfoStatusUpdateRegister>();
        services.AddScoped<SysLogRegister>();
        services.AddScoped<EventRegisterResolver>(x => key =>
        {
            return key switch
            {
                nameof(ConfigStatusUpdateRegister) => x.GetService<ConfigStatusUpdateRegister>(),
                nameof(ServiceInfoStatusUpdateRegister) => x.GetService<ServiceInfoStatusUpdateRegister>(),
                nameof(SysLogRegister) => x.GetService<SysLogRegister>(),
                _ => x.GetService<IEventRegister>()
            };
        });
        
        services.AddScoped<IServiceHealthCheckService, ServiceHealthCheckService>();
        services.AddScoped<IServiceInfoService, ServiceInfoService>();

        services.AddScoped<IAppService, AppService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IServerNodeService, ServerNodeService>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<ISysLogService, SysLogService>();
        services.AddScoped<IAppBasicAuthService, AppBasicAuthService>();
        services.AddScoped<IAdmBasicAuthService, AdmBasicAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPremissionService, PermissionService>();
        services.AddScoped<IRegisterCenterService, RegisterCenterService>();
    }
}