using AgileConfig.Server.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AgileConfig.Server.Data.Freesql
{
    public static class FreeSQL
    {
        private static IFreeSql _freesql;
        private static Dictionary<string, IFreeSql> _envFreesqls = new ();
        private static object _lock = new object();

        static FreeSQL()
        {
            _freesql = new FreeSql.FreeSqlBuilder()
                          .UseConnectionString(ProviderToFreesqlDbType(DbProvider), DbConnection)
                          .Build();
            FluentApi.Config(_freesql);
            EnsureTables.Ensure(_freesql);
        }

        public static IFreeSql Instance => _freesql;

        /// <summary>
        /// 根据环境配置的字符串返回freesql 实例
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IFreeSql GetInstance(string env)
        {
            if (string.IsNullOrEmpty(env))
            {
                return Instance;
            }

            var provider = Global.Config[$"db:env:{env}:provider"];
            var conn = Global.Config[$"db:env:{env}:conn"];

            var key = provider;

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
                        .UseConnectionString(ProviderToFreesqlDbType(provider), conn)
                        .Build();
                FluentApi.Config(sql);
                EnsureTables.Ensure(sql);

                _envFreesqls.Add(key, sql);

                return sql;
            }
        }

        private static string DbProvider => Global.Config["db:provider"];
        private static string DbConnection => Global.Config["db:conn"];
        
        private static FreeSql.DataType ProviderToFreesqlDbType(string provider)
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
                case "oracle":
                    return FreeSql.DataType.Oracle;
                default:
                    break;
            }

            return FreeSql.DataType.Sqlite;
        }
    }
}
