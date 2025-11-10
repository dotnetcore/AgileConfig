using System.Linq.Expressions;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Abstraction;

public interface IRepository<T, T1> : IDisposable where T : IEntity<T1>
{
    IUow Uow { get; set; }
    Task<List<T>> AllAsync();
    Task<T?> GetAsync(T1 id);

    Task UpdateAsync(T entity);
    Task UpdateAsync(IList<T> entities);

    Task DeleteAsync(T1 id);

    Task DeleteAsync(T entity);

    Task DeleteAsync(IList<T> entities);

    Task<T> InsertAsync(T entity);

    Task InsertAsync(IList<T> entities);

    Task<List<T>> QueryAsync(Expression<Func<T, bool>> exp);

    Task<List<T>> QueryPageAsync(Expression<Func<T, bool>> exp, int pageIndex, int pageSize,
        string defaultSortField = "Id", string defaultSortType = "ASC");

    Task<long> CountAsync(Expression<Func<T, bool>>? exp = null);
}