using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
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

    public T? Find(string id)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(string))
        {
            var definitionString = new StringFieldDefinition<T, string>("Id");
            var filter = Builders<T>.Filter.Eq(definitionString, id);
            return Collection.Find(filter).SingleOrDefault();
        }
        return default;
    }

    public async Task<T?> FindAsync(string id)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(string))
        {
            var definitionString = new StringFieldDefinition<T, string>("Id");
            var filter = Builders<T>.Filter.Eq(definitionString, id);
            return await (await Collection.FindAsync(filter)).SingleOrDefaultAsync();
        }
        return default;
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
        if (typeof(T).GetProperty("Id") == null)
            throw new Exception($"type {typeof(T)} no exists property 'id'.");
        var filter = Builders<T>.Filter.Eq(new StringFieldDefinition<T, string>("Id"), id);
        return await Collection.DeleteOneAsync(filter);
    }

    public async Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> predicate, UpdateDefinition<T> update)
    {
        var result = await Collection.UpdateManyAsync(predicate, update);
        return result;
    }

    public async Task<ReplaceOneResult> UpdateAsync(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new ArgumentException("In the entity no exists property 'id'.", nameof(entity));
        var id = idProperty.GetValue(entity);
        if (id == null)
            throw new ArgumentException("The entity property 'id' value is null.", nameof(entity));
        var idTypeName = idProperty.PropertyType.Name;
        FilterDefinition<T> filter;
        switch (idTypeName)
        {
            case "ObjectId":
                var definitionObjectId = new StringFieldDefinition<T, ObjectId>("Id");
                filter = Builders<T>.Filter.Eq(definitionObjectId, (ObjectId)id);
                break;
            case "Int32":
                var definitionInt32 = new StringFieldDefinition<T, int>("Id");
                filter = Builders<T>.Filter.Eq(definitionInt32, (int)id);
                break;
            case "String":
                var definitionString = new StringFieldDefinition<T, string>("Id");
                filter = Builders<T>.Filter.Eq(definitionString, (string)id);
                break;
            default:
                throw new Exception($"Do not support {idTypeName} type!");
        }

        var result = await Collection.ReplaceOneAsync(filter, entity);
        return result;
    }

    public async Task<long> UpdateAsync(IReadOnlyCollection<T> collection)
    {
        var rows = 0L;
        foreach (var item in collection)
        {
            var result = await UpdateAsync(item);
            rows += result.ModifiedCount;
        }

        return rows;
    }

    public ReplaceOneResult Update(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new ArgumentException("In the entity no exists property 'id'.", nameof(entity));
        var id = idProperty.GetValue(entity);
        if (id == null)
            throw new ArgumentException("The entity property 'id' value is null.", nameof(entity));
        var idTypeName = idProperty.PropertyType.Name;
        FilterDefinition<T> filter;
        switch (idTypeName)
        {
            case "ObjectId":
                var definitionObjectId = new StringFieldDefinition<T, ObjectId>("Id");
                filter = Builders<T>.Filter.Eq(definitionObjectId, (ObjectId)id);
                break;
            case "Int32":
                var definitionInt32 = new StringFieldDefinition<T, int>("Id");
                filter = Builders<T>.Filter.Eq(definitionInt32, (int)id);
                break;
            case "String":
                var definitionString = new StringFieldDefinition<T, string>("Id");
                filter = Builders<T>.Filter.Eq(definitionString, (string)id);
                break;
            default:
                throw new Exception($"Do not support {idTypeName} type!");
        }

        var result = Collection.ReplaceOne(filter, entity);
        return result;
    }

    public long Update(IReadOnlyCollection<T> collection)
    {
        var rows = 0L;
        foreach (var item in collection)
        {
            var result = Update(item);
            rows += result.ModifiedCount;
        }

        return rows;
    }
    
    
}