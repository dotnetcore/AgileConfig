using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Data.Mongodb;

public class Repository<T>  : IRepository<T> where T :  new()
{
    public IMongoDatabase Database => _access.Database;
    public IMongoClient Client => _access.Client;
    public IMongoCollection<T> Collection => _access.Collection;
    public IMongoQueryable<T> MongodbQueryable => _access.MongoQueryable;

    private readonly MongodbAccess<T> _access;

    public Repository(string? connectionString)
    {
        _access = new MongodbAccess<T>(connectionString);
    }
    
    [ActivatorUtilitiesConstructor]
    public Repository(IConfiguration configuration)
    {
        var connectionString = configuration["db:conn"];
        _access = new MongodbAccess<T>(connectionString);
    }
    
    public IMongoQueryable<T> SearchFor(Expression<Func<T, bool>> predicate)
    {
        return _access.MongoQueryable.Where(predicate);
    }

    public async IAsyncEnumerable<T> SearchForAsync(FilterDefinition<T> filter)
    {
        var result = await Collection.FindAsync(filter);
        while (await result.MoveNextAsync())
        {
            foreach (var item in result.Current)
            {
                yield return item;
            }
        }
    }

    public T? Find(object id)
    {
        var filter = GetIdPropertyFilter(id);
        return Collection.Find(filter).SingleOrDefault();
    }

    public async Task<T?> FindAsync(object id)
    {
        var filter = GetIdPropertyFilter(id);
        return await (await Collection.FindAsync(filter)).SingleOrDefaultAsync();
    }
    
    private static Expression<Func<T,bool>> GetIdPropertyFilter(object idValue)
    {
        var idProperty = GetIdProperty();
        if (idValue == null)
            throw new Exception($"The entity property '{idProperty.Name}' value is null.");
        
        var parameter = Expression.Parameter(typeof(T), "__q");
        var memberExpress = Expression.Property(parameter, idProperty);
        var expression =
            Expression.Lambda<Func<T, bool>>(Expression.Equal(memberExpress, Expression.Constant(idValue)),parameter);

        return expression;
    }

    public void Insert(params T[] source)
    {
        if(source.Length != 0)
            Collection.InsertMany(source);
    }

    public void Insert(IReadOnlyCollection<T> source)
    {
        if(source.Count != 0)
            Collection.InsertMany(source);
    }

    public async Task InsertAsync(params T[] source)
    {
        if(source.Length != 0)
            await Collection.InsertManyAsync(source);
    }

    public async Task InsertAsync(IReadOnlyCollection<T> source)
    {
        if(source.Count != 0)
            await Collection.InsertManyAsync(source);
    }

    public async Task<DeleteResult> DeleteAsync(Expression<Func<T, bool>> filter)
    {
        var result = await Collection.DeleteManyAsync(filter);
        return result;
    }

    public async Task<DeleteResult> DeleteAsync(FilterDefinition<T> filter)
    {
        var result = await Collection.DeleteManyAsync(filter);
        return result;
    }

    public async Task<DeleteResult> DeleteAsync(string id)
    {
        var filter = GetIdPropertyFilter(id);
        return await Collection.DeleteOneAsync(filter);
    }

    public async Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> predicate, UpdateDefinition<T> update)
    {
        var result = await Collection.UpdateManyAsync(predicate, update);
        return result;
    }

    private static PropertyInfo GetIdProperty()
    {
        var idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperties().FirstOrDefault(x =>
        {
            var attribute = x.GetCustomAttribute(typeof(BsonIdAttribute));
            return attribute != null;
        });
        if (idProperty == null)
            throw new Exception("In the entity no exists property 'id'.");
        return idProperty;
    }
    
    private static Expression<Func<T,bool>> GetIdPropertyFilter(T entity)
    {
        var idProperty = GetIdProperty();
        var idValue = idProperty.GetValue(entity);
        if (idValue == null)
            throw new ArgumentException($"The entity property '{idProperty.Name}' value is null.", nameof(entity));
        
        var parameter = Expression.Parameter(typeof(T), "__q");
        var memberExpress = Expression.Property(parameter, idProperty);
        var expression =
            Expression.Lambda<Func<T, bool>>(Expression.Equal(memberExpress, Expression.Constant(idValue)));

        return expression;
    }

    public async Task<ReplaceOneResult> UpdateAsync(T entity)
    {
        var filter = GetIdPropertyFilter(entity);
        var result = await Collection.ReplaceOneAsync(filter, entity);
        return result;
    }

    public async Task<BulkWriteResult<T>> UpdateAsync(IEnumerable<T> entities)
    {
        var writes = entities
            .Select(x => new ReplaceOneModel<T>(GetIdPropertyFilter(x), x))
            .ToList();
        return await Collection.BulkWriteAsync(writes);
    }

    public ReplaceOneResult Update(T entity)
    {
        var filter = GetIdPropertyFilter(entity);
        var result = Collection.ReplaceOne(filter, entity);
        return result;
    }

    public BulkWriteResult<T> Update(IEnumerable<T> entities)
    {
        var writes = entities
            .Select(x => new ReplaceOneModel<T>(GetIdPropertyFilter(x), x))
            .ToList();
        return Collection.BulkWrite(writes);
    }
    
    
}