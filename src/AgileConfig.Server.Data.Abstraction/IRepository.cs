using AgileConfig.Server.Common;
using System.Linq.Expressions;

namespace AgileConfig.Server.Data.Abstraction
{
    public interface IRepository<T, T1> : IDisposable where T : IEntity<T1>
    {
        Task<List<T>> AllAsync();
        Task<T> GetAsync(T1 id);

        Task UpdateAsync(T entity);
        Task UpdateAsync(IList<T> entities);

        Task DeleteAsync(T entity);

        Task DeleteAsync(IList<T> entities);

        Task<T> InsertAsync(T entity);

        Task InsertAsync(IList<T> entities);

        Task<List<T>> QueryAsync(Expression<Func<T, bool>> exp);
    }
}
