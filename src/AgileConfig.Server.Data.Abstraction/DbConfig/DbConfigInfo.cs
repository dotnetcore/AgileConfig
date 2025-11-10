namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public class DbConfigInfo : IDbConfigInfo
{
    public DbConfigInfo(string env, string provider, string conn)
    {
        Env = env;
        Provider = provider;
        ConnectionString = conn;
    }

    public string Env { get; }

    public string Provider { get; }

    public string ConnectionString { get; }
}