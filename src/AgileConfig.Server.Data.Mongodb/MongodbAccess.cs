using System.Collections.Concurrent;
using System.Security;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.Data.Mongodb;

public abstract class MongodbAccess
{
    private readonly record struct Db(string DatabaseName,IMongoClient Client);
    
    private static readonly Lazy<ConcurrentDictionary<string,Db>> LazyMongoClients = new();

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
            throw new Exception("没有配置Mongodb连接字符串");
        }

        _connectionString = connectionString;
        if (LazyMongoClients is not { IsValueCreated: true } || !LazyMongoClients.Value.ContainsKey(connectionString))
        {
            var url = MongoUrl.Create(connectionString);
            
            // 连接字符串如果不指定数据库名称，那么默认数据库名称就是 AgileConfig
            const string defaultDataBaseName = "AgileConfig";
            var databaseName = string.IsNullOrEmpty(url.DatabaseName) ? defaultDataBaseName : url.DatabaseName;
            LazyMongoClients.Value.TryAdd(connectionString, new Db(databaseName, new MongoClient(url)));
        }
    }

    /// <summary>
    /// 获取Mongodb客户端
    /// </summary>
    internal IMongoClient Client => LazyMongoClients.Value[_connectionString].Client ?? throw new Exception("IMongoClient value is null");

    /// <summary>
    /// 获取 MongoDB 数据库
    /// </summary>
    public IMongoDatabase Database => Client.GetDatabase(LazyMongoClients.Value[_connectionString].DatabaseName);
}

public sealed class MongodbAccess<T>(string? connectionString) : MongodbAccess(connectionString)
    where T : new()
{
    /// <summary>
    /// database collection name
    /// </summary>
    public string CollectionName => typeof(T).Name;

    /// <summary>
    /// 获取 该实体中 MongoDB数据库的集合
    /// </summary>
    public IMongoCollection<T> Collection => Database.GetCollection<T>(CollectionName);

    /// <summary>
    /// 获取 提供对MongoDB数据查询的Queryable
    /// </summary>
    /// <returns></returns>
    public IQueryable<T> MongoQueryable => Database.GetCollection<T>(CollectionName).AsQueryable(
        new AggregateOptions { AllowDiskUse = true });
}