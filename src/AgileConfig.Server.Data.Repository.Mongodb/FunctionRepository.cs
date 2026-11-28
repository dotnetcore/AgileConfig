namespace AgileConfig.Server.Data.Repository.Mongodb;

public class FunctionRepository : MongodbRepository<Function, string>, IFunctionRepository
{
    public FunctionRepository(string? connectionString, MongodbTlsOptions tlsOptions = null) : base(connectionString, tlsOptions)
    {
    }

    [ActivatorUtilitiesConstructor]
    public FunctionRepository(IConfiguration configuration) : base(configuration)
    {
    }
}