using System.Collections.Concurrent;
using System.Data.Common;
using System.Net.Sockets;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.Repository.Selector;

public sealed class DatabaseCircuitRegistry
{
    private readonly ConcurrentDictionary<(string Provider, string Connection), DatabaseCircuitBreaker> _circuits = new();
    private readonly IDbConfigInfoFactory _configFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DatabaseCircuitBreaker> _logger;
    private readonly int _failureThreshold;
    private readonly TimeSpan _breakDuration;

    public DatabaseCircuitRegistry(IDbConfigInfoFactory configFactory, TimeProvider timeProvider,
        ILogger<DatabaseCircuitBreaker> logger, IConfiguration configuration)
    {
        _configFactory = configFactory;
        _timeProvider = timeProvider;
        _logger = logger;
        _failureThreshold = configuration.GetValue("databaseResilience:failureThreshold", 3);
        var seconds = configuration.GetValue("databaseResilience:breakSeconds", 10);
        if (_failureThreshold <= 0 || seconds <= 0)
            throw new ArgumentException("Database resilience failureThreshold and breakSeconds must be positive.");
        _breakDuration = TimeSpan.FromSeconds(seconds);
    }

    public DatabaseCircuitBreaker Get(string environment = null)
    {
        var config = _configFactory.GetConfigInfo(environment);
        return _circuits.GetOrAdd((config.Provider.ToLowerInvariant(), config.ConnectionString),
            _ => new DatabaseCircuitBreaker(_timeProvider, _logger, IsConnectivityFailure,
                _failureThreshold, _breakDuration));
    }

    public static bool IsConnectivityFailure(Exception exception)
    {
        if (exception is DatabaseUnavailableException or OperationCanceledException) return false;
        if (exception is TimeoutException or SocketException or MongoConnectionException) return true;
        if (exception is DbException db && db.IsTransient) return true;
        if (exception is Microsoft.Data.SqlClient.SqlException sql &&
            sql.Number is -2 or 20 or 53 or 64 or 233 or 10053 or 10054 or 10060 or 10061 or 11001)
            return true;
        if (exception is System.Data.SQLite.SQLiteException sqlite &&
            sqlite.ResultCode is System.Data.SQLite.SQLiteErrorCode.Busy or System.Data.SQLite.SQLiteErrorCode.Locked
                or System.Data.SQLite.SQLiteErrorCode.CantOpen or System.Data.SQLite.SQLiteErrorCode.IoErr)
            return true;
        if (exception is Oracle.ManagedDataAccess.Client.OracleException oracle &&
            oracle.Number is 3113 or 3114 or 3135 or 12170 or 12514 or 12516 or 12518 or 12520
                or 12528 or 12537 or 12541 or 12543 or 12545 or 12547 or 12560 or 12571)
            return true;

        return exception.InnerException != null && IsConnectivityFailure(exception.InnerException);
    }
}
