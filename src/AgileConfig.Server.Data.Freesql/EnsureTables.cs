using System;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using FreeSql;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Data.Freesql;

public class EnsureTables
{
    private const string Sqlite_ExistTableSql =
        "SELECT count(1) FROM sqlite_master WHERE type='table' AND (name = 'agc_app' OR name = 'AGC_APP')";

    private const string Mysql_ExistTableSql =
        " SELECT count(1) FROM information_schema.TABLES WHERE table_schema= @schema AND (table_name ='agc_app' OR table_name='AGC_APP')";

    private const string SqlServer_ExistTableSql =
        "SELECT COUNT(1) FROM dbo.sysobjects WHERE ID = object_id(N'[dbo].[agc_app]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";

    private const string Oracle_ExistTableSql =
        "select count(1) from user_tables where table_name = 'agc_app' or table_name = 'AGC_APP'";

    private const string PostgreSql_ExistTableSql =
        "select count(1) from pg_class where relname = 'agc_app' or relname = 'AGC_APP'";

    public static bool ExistTable(IFreeSql instance)
    {
        //sqlite exist table?
        var sql = "";
        var schema = "";
        switch (instance.Ado.DataType)
        {
            case DataType.Sqlite:
                sql = Sqlite_ExistTableSql;
                break;
            case DataType.MySql:
                sql = Mysql_ExistTableSql;
                schema = instance.Ado.MasterPool.Get().Value.Database;
                break;
            case DataType.SqlServer:
                sql = SqlServer_ExistTableSql;
                break;
            case DataType.Oracle:
                sql = Oracle_ExistTableSql;
                break;
            case DataType.PostgreSQL:
                sql = PostgreSql_ExistTableSql;
                break;
            default:
                sql = Sqlite_ExistTableSql;
                break;
        }

        dynamic count = instance.Ado.ExecuteScalar(sql, new { schema });

        return count > 0;
    }

    /// <summary>
    ///     Create tables if they do not already exist.
    /// </summary>
    /// <param name="instance">FreeSql instance used to inspect schema state and create tables.</param>
    public static void Ensure(IFreeSql instance)
    {
        if (!ExistTable(instance))
        {
            try
            {
                if (instance.Ado.DataType == DataType.Oracle)
                    instance.CodeFirst.IsSyncStructureToUpper = true;

                instance.CodeFirst.SyncStructure<App>();
                instance.CodeFirst.SyncStructure<Config>();
                instance.CodeFirst.SyncStructure<ServerNode>();
                instance.CodeFirst.SyncStructure<Setting>();
                instance.CodeFirst.SyncStructure<SysLog>();
                instance.CodeFirst.SyncStructure<AppInheritanced>();
                instance.CodeFirst.SyncStructure<User>();
                instance.CodeFirst.SyncStructure<UserRole>();
                instance.CodeFirst.SyncStructure<Role>();
                instance.CodeFirst.SyncStructure<UserAppAuth>();
                instance.CodeFirst.SyncStructure<ConfigPublished>();
                instance.CodeFirst.SyncStructure<PublishTimeline>();
                instance.CodeFirst.SyncStructure<PublishDetail>();
                instance.CodeFirst.SyncStructure<ServiceInfo>();
                instance.CodeFirst.SyncStructure<Function>();
                instance.CodeFirst.SyncStructure<RoleFunction>();
            }
            catch (Exception ex)
            {
                var logger = Global.LoggerFactory.CreateLogger<EnsureTables>();
                logger.LogError(ex, "Ensure Tables failed .");
            }
        }
    }
}
