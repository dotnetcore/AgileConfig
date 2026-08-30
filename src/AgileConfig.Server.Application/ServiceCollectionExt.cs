using System;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Application.Roles;
using AgileConfig.Server.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Application;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IApplicationManagementService, ApplicationManagementService>();
        services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
        services.AddScoped<IPublishedConfigurationQueryService, PublishedConfigurationQueryService>();
        services.AddScoped<IReleaseManagementService, ReleaseManagementService>();
        services.AddScoped<INodeManagementService, NodeManagementService>();
        services.AddScoped<IServiceInstanceManagementService, ServiceInstanceManagementService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        return services;
    }
}
