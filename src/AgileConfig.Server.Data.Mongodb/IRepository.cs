using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Data.Mongodb;

public interface IRepository
{
    IMongoDatabase Database { get; }
}

public interface IRepository<T> : IRepository where T : new()
{
    /// <summary>
    /// mongodb客户端
    /// </summary>
    IMongoClient Client { get; }

    /// <summary>
    /// 获取 该实体Mongodb的集合
    /// </summary>
    IMongoCollection<T> Collection { get; }

    /// <summary>
    /// Mongodb queryable
    /// </summary>
    IMongoQueryable<T> MongodbQueryable { get; }

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IMongoQueryable<T> SearchFor(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// 异步 数据查询
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IAsyncEnumerable<T> SearchForAsync(FilterDefinition<T> filter);

    /// <summary>
    /// 根据 Id 获取单条记录，不存在将会返回null
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T? Find(object id);

    /// <summary>
    /// 根据 Id 获取单条记录，不存在将会返回null
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T?> FindAsync(object id);

    void Insert(params T[] source);
    
    void Insert(IReadOnlyCollection<T> source);

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    Task InsertAsync(params T[] source);

    /// <summary>
    /// 插入数据
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    Task InsertAsync(IReadOnlyCollection<T> source);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<DeleteResult> DeleteAsync(Expression<Func<T, bool>> filter);

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter);

    /// <summary>
    /// 根据ID删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<DeleteResult> DeleteAsync(string id);

    /// <summary>
    /// 根据查询条件更新数据
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="update"></param>
    /// <returns></returns>
    Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> predicate, UpdateDefinition<T> update);

    /// <summary>
    /// 根据实体修改数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <exception cref="ArgumentException">If the entity no exists property 'id',then will throw exception.</exception>
    Task<ReplaceOneResult> UpdateAsync(T entity);

    Task<BulkWriteResult<T>> UpdateAsync(IEnumerable<T> entities);
    
    ReplaceOneResult Update(T entity);
    
    BulkWriteResult<T> Update(IEnumerable<T> entities);
}