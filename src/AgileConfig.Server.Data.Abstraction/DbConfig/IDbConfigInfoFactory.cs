namespace AgileConfig.Server.Data.Abstraction.DbProvider;

public interface IDbConfigInfoFactory
{
    IDbConfigInfo GetConfigInfo(string env = "");
}