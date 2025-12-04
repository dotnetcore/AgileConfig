namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public interface IDbConfigInfo
{
    string ConnectionString { get; }
    string Env { get; }

    string Provider { get; }
}