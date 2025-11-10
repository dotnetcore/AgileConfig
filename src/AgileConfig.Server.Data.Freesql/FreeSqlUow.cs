using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using FreeSql;

namespace AgileConfig.Server.Data.Freesql;

public class FreeSqlUow : IUow
{
    private readonly IFreeSql _freeSql;
    private readonly IUnitOfWork _unitOfWork;


    public FreeSqlUow(IFreeSql freeSql)
    {
        _freeSql = freeSql;
        _unitOfWork = _freeSql.CreateUnitOfWork();
    }

    public Task<bool> SaveChangesAsync()
    {
        _unitOfWork.Commit();

        return Task.FromResult(true);
    }

    public void Rollback()
    {
        _unitOfWork?.Rollback();
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
    }

    public void Begin()
    {
        // FreeSql unit of work does not require a manual begin call.
    }

    public IUnitOfWork GetFreesqlUnitOfWork()
    {
        return _unitOfWork;
    }
}