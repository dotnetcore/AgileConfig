namespace AgileConfig.Server.Mongodb.ServiceTest;

public class DatabaseFixture
{
    private const string ConnectionString = "mongodb://localhost:27017,localhost:37017/AgileConfig";

    protected IRepository<App> AppRepository => new Repository<App>(ConnectionString);

    protected IRepository<AppInheritanced> AppInheritancedRepository =>
        new Repository<AppInheritanced>(ConnectionString);

    protected IRepository<Config> ConfigRepository => new Repository<Config>(ConnectionString);

    protected IRepository<ConfigPublished> ConfigPublishedRepository =>
        new Repository<ConfigPublished>(ConnectionString);

    protected IRepository<UserAppAuth> UserAppAuthRepository => new Repository<UserAppAuth>(ConnectionString);

    protected IRepository<User> UserRepository => new Repository<User>(ConnectionString);

    protected IRepository<Setting> SettingRepository => new Repository<Setting>(ConnectionString);

    protected IRepository<UserRole> UserRoleRepository => new Repository<UserRole>(ConnectionString);

    protected IRepository<ServerNode> ServerNodeRepository => new Repository<ServerNode>(ConnectionString);
    
    protected IRepository<SysLog> SysLogRepository => new Repository<SysLog>(ConnectionString);
}