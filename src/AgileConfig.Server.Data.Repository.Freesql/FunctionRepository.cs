using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql;

public class FunctionRepository : FreesqlRepository<Function, string>, IFunctionRepository
{
    private readonly IFreeSqlFactory freeSqlFactory;

    public FunctionRepository(IFreeSqlFactory freeSqlFactory) : base(freeSqlFactory.Create())
    {
        this.freeSqlFactory = freeSqlFactory;
    }
}