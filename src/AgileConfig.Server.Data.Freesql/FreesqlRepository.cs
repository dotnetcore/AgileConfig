using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AgileConfig.Server.Data.Freesql
{
    public abstract class FreesqlRepository<T, T1> : IRepository<T, T1> where T : class, IEntity<T1>
    {
        private readonly IBaseRepository<T> _repository;

        public FreesqlRepository(IFreeSql freeSql)
        {
            _repository = freeSql.GetRepository<T>();
        }

        public Task<List<T>> AllAsync()
        {
            return _repository.Select.ToListAsync();
        }

        public async Task DeleteAsync(T1 id)
        {
            await _repository.DeleteAsync(x => Equals(x.Id, id));
        }

        public Task DeleteAsync(T entity)
        {
            return _repository.DeleteAsync(entity);
        }

        public Task DeleteAsync(IList<T> entities)
        {
            return _repository.DeleteAsync(entities);
        }

        public Task<T?> GetAsync(T1 id)
        {
            return _repository.Where(x => x.Id.Equals(id)).ToOneAsync();
        }

        public Task<T> InsertAsync(T entity)
        {
            return _repository.InsertAsync(entity);
        }

        public Task InsertAsync(IList<T> entities)
        {
            return _repository.InsertAsync(entities);
        }

        public Task<List<T>> QueryAsync(Expression<Func<T, bool>> exp)
        {
            return _repository.Where(exp).ToListAsync();
        }

        public async Task<List<T>> QueryPageAsync(Expression<Func<T, bool>> exp, int pageIndex, int pageSize, string defaultSortField = "Id",
            string defaultSortType = "ASC")
        {
            var list = await _repository.Where(exp)
                .OrderByPropertyName(defaultSortField, defaultSortType.Equals("ASC", StringComparison.OrdinalIgnoreCase))
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return list;
        }


        public async Task<long> CountAsync(Expression<Func<T, bool>>? exp = null)
        {
            return await (exp == null ? _repository.Select.CountAsync() : _repository.Select.Where(exp).CountAsync());
        }

        public Task UpdateAsync(T entity)
        {
            return _repository.UpdateAsync(entity);
        }

        public Task UpdateAsync(IList<T> entities)
        {
            return _repository.UpdateAsync(entities);
        }

        public void Dispose()
        {
            _repository.Dispose();
        }
    }
}
