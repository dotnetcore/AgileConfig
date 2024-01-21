using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public static class ServiceCollectionExt
    {
        public static void AddFreeSqlRepository(this IServiceCollection sc)
        {
            sc.AddFreeRepository();

            sc.AddScoped<IAppInheritancedRepository, AppInheritancedRepository>();
            sc.AddScoped<IAppRepository, AppRepository>();
            sc.AddScoped<IConfigPublishedRepository, ConfigPublishedRepository>();
            sc.AddScoped<IConfigRepository, ConfigRepository>();
            sc.AddScoped<IPublishDetailRepository, PublishDetailRepository>();
            sc.AddScoped<IPublishTimelineRepository, PublishTimelineRepository>();
            sc.AddScoped<IServerNodeRepository, ServerNodeRepository>();
            sc.AddScoped<IServiceInfoRepository, ServiceInfoRepository>();
            sc.AddScoped<ISettingRepository, SettingRepository>();
            sc.AddScoped<ISysLogRepository, SysLogRepository>();
            sc.AddScoped<IUserAppAuthRepository, UserAppAuthRepository>();
            sc.AddScoped<IUserRepository, UserRepository>();
            sc.AddScoped<IUserRoleRepository, UserRoleRepository>();
            
            sc.AddSingleton<ISysInitRepository, SysInitRepository>();
        }
    }
}
