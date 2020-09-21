using AgileConfig.Server.Common;
using Microsoft.Extensions.Configuration;
using System;

namespace AgileConfig.Server.Data.Freesql
{
    public static class FreeSqlContainer
    {
        private static IFreeSql _freesql;

        static FreeSqlContainer()
        {
            _freesql = new FreeSql.FreeSqlBuilder()
                          .UseConnectionString(ProviderToFreesqlDbType(DbProvider), DbConnection)
                          .Build(); //请务必定义成 Singleton 单例模式
        }

        public static IFreeSql FreeSqlInstance => _freesql;

        private static string DbProvider => Configuration.Config["db:provider"];
        private static string DbConnection => Configuration.Config["db:conn"];
        
        private static FreeSql.DataType ProviderToFreesqlDbType(string provider)
        {
            switch (provider)
            {
                case "sqlite":
                    return FreeSql.DataType.Sqlite;
                case "mysql":
                    return FreeSql.DataType.MySql;
                case "sqlserver":
                    return FreeSql.DataType.SqlServer;
                case "npgsql":
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
