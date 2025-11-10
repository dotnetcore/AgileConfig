using System.Collections.Concurrent;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Mongodb;

public abstract class MongodbAccess
{
    private static readonly Lazy<ConcurrentDictionary<string, Db>> LazyMongoClients = new();

    private readonly string _connectionString;

    internal MongodbAccess(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            if (!LazyMongoClients.Value.IsEmpty)
            {
                _connectionString = LazyMongoClients.Value.First().Key;
                return;
            }

            throw new Exception("MongoDB connection string is not configured.");
        }

        _connectionString = connectionString;
        if (LazyMongoClients is not { IsValueCreated: true } || !LazyMongoClients.Value.ContainsKey(connectionString))
        {
            var url = MongoUrl.Create(connectionString);

            // Use "AgileConfig" as the default database name when it is not specified in the connection string.
            const string defaultDataBaseName = "AgileConfig";
            var databaseName = string.IsNullOrEmpty(url.DatabaseName) ? defaultDataBaseName : url.DatabaseName;
            LazyMongoClients.Value.TryAdd(connectionString, new Db(databaseName, new MongoClient(url)));
        }
    }

    /// <summary>
    ///     Get the MongoDB client instance.
    /// </summary>
    internal IMongoClient Client => LazyMongoClients.Value[_connectionString].Client ??
                                    throw new Exception("IMongoClient value is null");

    /// <summary>
    ///     Get the MongoDB database instance.
    /// </summary>
    public IMongoDatabase Database => Client.GetDatabase(LazyMongoClients.Value[_connectionString].DatabaseName);

    private readonly record struct Db(string DatabaseName, IMongoClient Client);
}

public sealed class MongodbAccess<T>(string? connectionString) : MongodbAccess(connectionString)
    where T : new()
{
    /// <summary>
    ///     database collection name
    /// </summary>
    public string CollectionName => typeof(T).Name;

    /// <summary>
    ///     Get the MongoDB collection for the entity type.
    /// </summary>
    public IMongoCollection<T> Collection => Database.GetCollection<T>(CollectionName);

    /// <summary>
    ///     Get an IQueryable interface for querying MongoDB data.
    /// </summary>
    /// <returns></returns>
    public IQueryable<T> MongoQueryable => Database.GetCollection<T>(CollectionName).AsQueryable(
        new AggregateOptions { AllowDiskUse = true });
}