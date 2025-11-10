namespace AgileConfig.Server.Data.Repository.Mongodb;

public class ServerNodeRepository : MongodbRepository<ServerNode, string>, IServerNodeRepository
{
    public ServerNodeRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ServerNodeRepository(IConfiguration configuration) : base(configuration)
    {
    }
}