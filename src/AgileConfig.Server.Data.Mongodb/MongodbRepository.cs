using System.Linq.Expressions;
using AgileConfig.Server.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Data.Mongodb;

public class MongodbRepository<TEntity, TId> : Abstraction.IRepository<TEntity, TId>
    where TEntity : IEntity<TId>, new()
{
    private readonly MongodbAccess<TEntity> _access;

    public MongodbRepository(string? connectionString)
    {
        _access = new MongodbAccess<TEntity>(connectionString);
    }

    [ActivatorUtilitiesConstructor]
    public MongodbRepository(IConfiguration configuration)
    {
        var connectionString = configuration["db:conn"];
        _access = new MongodbAccess<TEntity>(connectionString);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<List<TEntity>> AllAsync()
    {
        return await _access.MongoQueryable.ToListAsync();
    }
    
    private Expression<Func<TEntity,bool>> GetIdPropertyFilter(TId id)
    {
        var expression = _access.MongoQueryable.Where(x => Equals(x.Id, id)).Expression;
        return Expression.Lambda<Func<TEntity, bool>>(expression);
    }

    public async Task<TEntity> GetAsync(TId id)
    {
        var filter = GetIdPropertyFilter(id);
        return await (await _access.Collection.FindAsync(filter)).SingleOrDefaultAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        var filter = GetIdPropertyFilter(entity.Id);
        await _access.Collection.ReplaceOneAsync(filter, entity);
    }

    public async Task UpdateAsync(IList<TEntity> entities)
    {
        var writes = entities
            .Select(x => new ReplaceOneModel<TEntity>(GetIdPropertyFilter(x.Id), x))
            .ToList();
        await _access.Collection.BulkWriteAsync(writes);
    }

    public async Task DeleteAsync(TEntity entity)
    {
        var filter = GetIdPropertyFilter(entity.Id);
        await _access.Collection.DeleteOneAsync(filter);
    }

    public async Task DeleteAsync(IList<TEntity> entities)
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, entities.Select(y => y.Id));
        await _access.Collection.DeleteManyAsync(filter);
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        await _access.Collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task InsertAsync(IList<TEntity> entities)
    {
        if(entities.Count > 0)
            await _access.Collection.InsertManyAsync(entities);
    }

    public async Task<List<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> exp)
    {
        return await _access.MongoQueryable.Where(exp).ToListAsync();
    }
}