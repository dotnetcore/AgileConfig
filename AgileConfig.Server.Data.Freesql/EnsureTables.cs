using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public class EnsureTables
    {
        private const string Sqlite_ExistTableSql = "SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'app'";
        private const string Mysql_ExistTableSql = "SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'app'";
        private const string SqlServer_ExistTableSql = "SELECT COUNT(1) FROM dbo.SYSOBJECTS WHERE ID = object_id(N'[dbo].[app]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
        private const string Oracle_ExistTableSql = "SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'app'";
        private const string PostgreSql_ExistTableSql = "SELECT count(1) FROM sqlite_master WHERE type='table' AND name = 'app'";

        public static bool ExistTable()
        {
            //sqlite exist table?
            string sql = "";
            switch (FreeSQL.Instance.Ado.DataType)
            {
                case FreeSql.DataType.Sqlite:
                    sql = Sqlite_ExistTableSql;
                    break;
                case FreeSql.DataType.MySql:
                    sql = Mysql_ExistTableSql;
                    break;
                case FreeSql.DataType.SqlServer:
                    sql = SqlServer_ExistTableSql;
                    break;
                case FreeSql.DataType.Oracle:
                    sql = Oracle_ExistTableSql;
                    break;
                case FreeSql.DataType.PostgreSQL:
                    sql = PostgreSql_ExistTableSql;
                    break;
                default:
                    sql = Sqlite_ExistTableSql;
                    break;
            }

            dynamic count = FreeSQL.Instance.Ado.ExecuteScalar(sql);

            return count > 0;
        }

        public static void Ensure()
        {
            if (!ExistTable())
            {
                FreeSQL.Instance.CodeFirst.SyncStructure<App>();
                FreeSQL.Instance.CodeFirst.SyncStructure<Config>();
                FreeSQL.Instance.CodeFirst.SyncStructure<ModifyLog>();
                FreeSQL.Instance.CodeFirst.SyncStructure<ServerNode>();
                FreeSQL.Instance.CodeFirst.SyncStructure<Setting>();
                FreeSQL.Instance.CodeFirst.SyncStructure<SysLog>();
            }
        }
    }
}
