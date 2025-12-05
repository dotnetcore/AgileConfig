using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using FreeSql;

namespace AgileConfig.Server.Data.Freesql;

public class MyFreeSQL : IMyFreeSQL
{
    private static readonly object _lock = new();
    private readonly IDbConfigInfoFactory _dbConfigInfoFactory;
    private readonly Dictionary<string, IFreeSql> _envFreesqls = new();

    public MyFreeSQL(IDbConfigInfoFactory dbConfigInfoFactory)
    {
        _dbConfigInfoFactory = dbConfigInfoFactory;
    }

    /// <summary>
    ///     Return a FreeSql instance according to the connection settings of the specified environment.
    /// </summary>
    /// <param name="env">Environment name whose database connection should be used.</param>
    /// <returns>FreeSql instance configured for the specified environment.</returns>
    public IFreeSql GetInstanceByEnv(string env)
    {
        var dbConfig = _dbConfigInfoFactory.GetConfigInfo(env);

        var dbType = ProviderToFreesqlDbType(dbConfig.Provider);
        if (!dbType.HasValue)
            throw new ArgumentException(nameof(dbConfig.Provider),
                $"[{dbConfig.Provider}] is not a freesql supported provider.");

        var key = dbConfig.ConnectionString;

        if (_envFreesqls.ContainsKey(key)) return _envFreesqls[key];

        lock (_lock)
        {
            if (_envFreesqls.ContainsKey(key)) return _envFreesqls[key];

            var sql = new FreeSqlBuilder()
                .UseConnectionString(dbType.Value, dbConfig.ConnectionString)
                .Build();
            ApplyDatabaseStructrue(sql);

            _envFreesqls.Add(key, sql);

            return sql;
        }
    }

    private static void ApplyDatabaseStructrue(IFreeSql sql)
    {
        FluentApi.Config(sql);
        EnsureTables.Ensure(sql);
    }

    public static DataType? ProviderToFreesqlDbType(string provider)
    {
        switch (provider.ToLower())
        {
            case "sqlite":
                return DataType.Sqlite;
            case "mysql":
                return DataType.MySql;
            case "sqlserver":
                return DataType.SqlServer;
            case "npgsql":
                return DataType.PostgreSQL;
            case "postgresql":
                return DataType.PostgreSQL;
            case "pg":
                return DataType.PostgreSQL;
            case "oracle":
                return DataType.Oracle;
        }

        return null;
    }
}