namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ServerNodeRepository : MongodbRepository<ServerNode, string>, IServerNodeRepository
{
    public ServerNodeRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ServerNodeRepository(IConfiguration configuration) : base(configuration)
    {
    }
}