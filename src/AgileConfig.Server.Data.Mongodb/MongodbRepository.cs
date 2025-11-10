using System.Linq.Expressions;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Data.Mongodb;

public class MongodbRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : IEntity<TId>, new()
{
    private readonly MongodbAccess<TEntity> _access;

    private MongodbUow? _mongodbUow;

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

    public IUow Uow
    {
        get => _mongodbUow;
        set
        {
            _mongodbUow = value as MongodbUow;
            if (_mongodbUow?.Session == null && _mongodbUow?.Session?.IsInTransaction != true)
                _mongodbUow?.SetSession(_access.Client.StartSession());
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<List<TEntity>> AllAsync()
    {
        return await _access.MongoQueryable.ToListAsync();
    }

    public async Task<TEntity?> GetAsync(TId id)
    {
        var filter = GetIdPropertyFilter(id);
        return await (await _access.Collection.FindAsync(filter)).SingleOrDefaultAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (_mongodbUow?.Session == null)
        {
            var filter = GetIdPropertyFilter(entity.Id);
            await _access.Collection.ReplaceOneAsync(filter, entity);
        }
        else
        {
            var filter = GetIdPropertyFilter(entity.Id);
            await _access.Collection.ReplaceOneAsync(_mongodbUow.Session, filter, entity);
        }
    }

    public async Task UpdateAsync(IList<TEntity> entities)
    {
        var writes = entities
            .Select(x => new ReplaceOneModel<TEntity>(GetIdPropertyFilter(x.Id), x))
            .ToList();
        if (writes.Count > 0)
        {
            if (_mongodbUow?.Session == null)
                await _access.Collection.BulkWriteAsync(writes);
            else
                await _access.Collection.BulkWriteAsync(_mongodbUow.Session, writes);
        }
    }

    public async Task DeleteAsync(TId id)
    {
        var filter = Builders<TEntity>.Filter.Eq(x => x.Id, id);
        if (_mongodbUow?.Session == null)
            await _access.Collection.DeleteOneAsync(filter);
        else
            await _access.Collection.DeleteOneAsync(_mongodbUow.Session, filter);
    }

    public async Task DeleteAsync(TEntity entity)
    {
        var filter = GetIdPropertyFilter(entity.Id);
        if (_mongodbUow?.Session == null)
            await _access.Collection.DeleteOneAsync(filter);
        else
            await _access.Collection.DeleteOneAsync(_mongodbUow.Session, filter);
    }

    public async Task DeleteAsync(IList<TEntity> entities)
    {
        var filter = Builders<TEntity>.Filter.In(x => x.Id, entities.Select(y => y.Id));
        if (_mongodbUow?.Session == null)
            await _access.Collection.DeleteManyAsync(filter);
        else
            await _access.Collection.DeleteManyAsync(_mongodbUow.Session, filter);
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        if (_mongodbUow?.Session == null)
            await _access.Collection.InsertOneAsync(entity);
        else
            await _access.Collection.InsertOneAsync(_mongodbUow.Session, entity);

        return entity;
    }

    public async Task InsertAsync(IList<TEntity> entities)
    {
        if (entities.Count > 0)
        {
            if (_mongodbUow?.Session == null)
                await _access.Collection.InsertManyAsync(entities);
            else
                await _access.Collection.InsertManyAsync(_mongodbUow.Session, entities);
        }
    }

    public async Task<List<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> exp)
    {
        return await _access.MongoQueryable.Where(exp).ToListAsync();
    }

    public async Task<List<TEntity>> QueryPageAsync(Expression<Func<TEntity, bool>> exp, int pageIndex, int pageSize,
        string defaultSortField = "Id",
        string defaultSortType = "ASC")
    {
        if (pageIndex < 1)
            return new List<TEntity>();
        if (pageSize <= 0)
            return new List<TEntity>();
        var query = _access.MongoQueryable.Where(exp);
        var sort = Sort(defaultSortField);
        query = string.Equals(defaultSortType, "DESC", StringComparison.OrdinalIgnoreCase)
            ? query.OrderByDescending(sort)
            : query.OrderBy(sort);
        return await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? exp = null)
    {
        return await (exp == null
            ? _access.MongoQueryable.CountAsync()
            : _access.MongoQueryable.Where(exp).CountAsync());
    }

    private Expression<Func<TEntity, bool>> GetIdPropertyFilter(TId id)
    {
        Expression<Func<TEntity, bool>> expression = x => Equals(x.Id, id);
        return expression;
    }

    private static Expression<Func<TEntity, object>> Sort(string defaultSortField)
    {
        Expression<Func<TEntity, object>> defaultSort = x => x.Id;
        if (!string.IsNullOrEmpty(defaultSortField) &&
            !defaultSortField.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            var property = typeof(TEntity).GetProperty(defaultSortField);
            if (property == null) return defaultSort;

            var parameter = Expression.Parameter(typeof(TEntity), "__q");
            var memberExpress = Expression.Property(parameter, property);
            var convertExpress = Expression.Convert(memberExpress, typeof(object));
            return Expression.Lambda<Func<TEntity, object>>(convertExpress, parameter);
        }

        return defaultSort;
    }
}