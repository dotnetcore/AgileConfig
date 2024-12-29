﻿using AgileConfig.Server.Data.Abstraction.DbProvider;
using System;
using System.Collections.Generic;

namespace AgileConfig.Server.Data.Freesql
{
    public class MyFreeSQL : IMyFreeSQL
    {
        private Dictionary<string, IFreeSql> _envFreesqls = new();
        private static object _lock = new object();
        private readonly IDbConfigInfoFactory _dbConfigInfoFactory;

        public MyFreeSQL(IDbConfigInfoFactory dbConfigInfoFactory)
        {
            this._dbConfigInfoFactory = dbConfigInfoFactory;
        }

        /// <summary>
        /// 根据环境配置的字符串返回freesql 实例
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public IFreeSql GetInstanceByEnv(string env)
        {
            var dbConfig = _dbConfigInfoFactory.GetConfigInfo(env);

            var dbType = ProviderToFreesqlDbType(dbConfig.Provider);
            if (!dbType.HasValue)
            {
                throw new ArgumentException(nameof(dbConfig.Provider), $"[{dbConfig.Provider}] is not a freesql supported provider.");
            }

            var key = dbConfig.ConnectionString;

            if (_envFreesqls.ContainsKey(key))
            {
                return _envFreesqls[key];
            }

            lock (_lock)
            {
                if (_envFreesqls.ContainsKey(key))
                {
                    return _envFreesqls[key];
                }

                var sql = new FreeSql.FreeSqlBuilder()
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

        public static FreeSql.DataType? ProviderToFreesqlDbType(string provider)
        {
            switch (provider.ToLower())
            {
                case "sqlite":
                    return FreeSql.DataType.Sqlite;
                case "mysql":
                    return FreeSql.DataType.MySql;
                case "sqlserver":
                    return FreeSql.DataType.SqlServer;
                case "npgsql":
                    return FreeSql.DataType.PostgreSQL;
                case "postgresql":
                    return FreeSql.DataType.PostgreSQL;
                case "pg":
                    return FreeSql.DataType.PostgreSQL;
                case "oracle":
                    return FreeSql.DataType.Oracle;
                default:
                    break;
            }

            return null;
        }
    }
}
