using System.Collections.Concurrent;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Mongodb;

public abstract class MongodbAccess
{
    private static readonly Lazy<ConcurrentDictionary<string, Db>> LazyMongoClients = new();

    private readonly string _cacheKey;

    internal MongodbAccess(string? connectionString, MongodbTlsOptions tlsOptions = null)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            if (!LazyMongoClients.Value.IsEmpty)
            {
                _cacheKey = LazyMongoClients.Value.First().Key;
                return;
            }

            throw new Exception("MongoDB connection string is not configured.");
        }

        // "tlsCAFile" and friends are accepted by most MongoDB drivers but not by the .NET one.
        // They are extracted here and translated into the certificate validation callback below.
        var normalized = MongodbConnectionString.Normalize(connectionString);
        var effectiveTlsOptions = MongodbTlsOptions.Merge(tlsOptions, normalized.TlsOptions);
        _cacheKey = BuildCacheKey(normalized.ConnectionString, effectiveTlsOptions);

        if (!LazyMongoClients.Value.ContainsKey(_cacheKey))
        {
            var url = MongoUrl.Create(normalized.ConnectionString);

            // Use "AgileConfig" as the default database name when it is not specified in the connection string.
            const string defaultDataBaseName = "AgileConfig";
            var databaseName = string.IsNullOrEmpty(url.DatabaseName) ? defaultDataBaseName : url.DatabaseName;
            LazyMongoClients.Value.TryAdd(_cacheKey, new Db(databaseName, CreateClient(url, effectiveTlsOptions)));
        }
    }

    /// <summary>
    ///     Get the MongoDB client instance.
    /// </summary>
    internal IMongoClient Client => LazyMongoClients.Value[_cacheKey].Client ??
                                    throw new Exception("IMongoClient value is null");

    /// <summary>
    ///     Get the MongoDB database instance.
    /// </summary>
    public IMongoDatabase Database => Client.GetDatabase(LazyMongoClients.Value[_cacheKey].DatabaseName);

    private static string BuildCacheKey(string connectionString, MongodbTlsOptions tlsOptions)
    {
        return tlsOptions is null || tlsOptions.IsEmpty
            ? connectionString
            : $"{connectionString}|tls:{tlsOptions.CacheKey}";
    }

    private static IMongoClient CreateClient(MongoUrl url, MongodbTlsOptions tlsOptions)
    {
        if (tlsOptions is null || tlsOptions.IsEmpty) return new MongoClient(url);

        var settings = MongoClientSettings.FromUrl(url);

        // A CA was configured on purpose, so an encrypted connection is expected even when the
        // connection string forgot to append "tls=true".
        if (!settings.UseTls) settings.UseTls = true;

        var sslSettings = (settings.SslSettings ?? new SslSettings()).Clone();
        sslSettings.ServerCertificateValidationCallback = MongodbServerCertificateValidator.Create(tlsOptions);
        settings.SslSettings = sslSettings;

        return new MongoClient(settings);
    }

    private readonly record struct Db(string DatabaseName, IMongoClient Client);
}

public sealed class MongodbAccess<T>(string? connectionString, MongodbTlsOptions tlsOptions = null)
    : MongodbAccess(connectionString, tlsOptions)
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
