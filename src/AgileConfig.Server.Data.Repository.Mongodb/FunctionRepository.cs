namespace AgileConfig.Server.Data.Repository.Mongodb;

public class FunctionRepository : MongodbRepository<Function, string>, IFunctionRepository
{
    public FunctionRepository(string? connectionString) : base(connectionString)
    {
    }

    [ActivatorUtilitiesConstructor]
    public FunctionRepository(IConfiguration configuration) : base(configuration)
    {
    }
}