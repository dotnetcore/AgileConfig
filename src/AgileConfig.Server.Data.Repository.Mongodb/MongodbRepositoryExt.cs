namespace AgileConfig.Server.Data.Repository.Mongodb;

public static class MongodbRepositoryExt
{
    public static void AddMongodbRepository(this IServiceCollection services)
    {
        services.AddScoped<IAppInheritancedRepository, AppInheritancedRepository>();
        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<IConfigPublishedRepository, ConfigPublishedRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IPublishDetailRepository, PublishDetailRepository>();
        services.AddScoped<IPublishTimelineRepository, PublishTimelineRepository>();
        services.AddScoped<IServerNodeRepository, ServerNodeRepository>();
        services.AddScoped<IServiceInfoRepository, ServiceInfoRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<ISysLogRepository, SysLogRepository>();
        services.AddScoped<IUserAppAuthRepository, UserAppAuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, IUserRoleRepository>();
    }
}