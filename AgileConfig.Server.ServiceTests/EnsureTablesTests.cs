using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Data.Freesql;
using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Data.Freesql.Tests
{
    [TestClass()]
    public class EnsureTablesTests
    {
        [TestMethod()]
        public void ExistTableSqliteTest()
        {
            //sqlite
            string conn = "Data Source=agile_config.db";
            var sqllite_fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.Sqlite, conn)
                          .Build();
            FluentApi.Config(sqllite_fsq);
            sqllite_fsq.Ado.ExecuteNonQuery("drop table agc_app");
            var ex = EnsureTables.ExistTable(sqllite_fsq);
            Assert.IsFalse(ex);
            sqllite_fsq.CodeFirst.SyncStructure<App>();
            ex = EnsureTables.ExistTable(sqllite_fsq);
            Assert.IsTrue(ex);
        }

        [TestMethod()]
        public void ExistTableSqlServerTest()
        {
            //SqlServer
            string conn = "Persist Security Info = False; User ID =dev; Password =dev@123,; Initial Catalog =agile_config_test; Server =www..com";
            var sqlserver_fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.SqlServer, conn)
                          .Build();
            FluentApi.Config(sqlserver_fsq);
            sqlserver_fsq.Ado.ExecuteNonQuery("drop table agc_app");
            var ex = EnsureTables.ExistTable(sqlserver_fsq);
            Assert.IsFalse(ex);
            sqlserver_fsq.CodeFirst.SyncStructure<App>();
            ex = EnsureTables.ExistTable(sqlserver_fsq);
            Assert.IsTrue(ex);
        }

        [TestMethod()]
        public void ExistTableMysqlTest()
        {
            //SqlServer
            string conn = "Database=agile_config_test;Data Source=localhost;User Id=root;Password=dev@123;port=3306";
            var mysql_fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.MySql, conn)
                          .Build();
            FluentApi.Config(mysql_fsq);
            mysql_fsq.Ado.ExecuteNonQuery("drop table agc_app");
            var ex = EnsureTables.ExistTable(mysql_fsq);
            Assert.IsFalse(ex);
            mysql_fsq.CodeFirst.SyncStructure<App>();
            ex = EnsureTables.ExistTable(mysql_fsq);
            Assert.IsTrue(ex);
        }

        [TestMethod()]
        public void ExistTableOracleTest()
        {
            //SqlServer
            string conn = "user id=CLINIC;password=CLINIC;data source=192.168.0.91/orcl";
            var oracle_fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.Oracle, conn)
                          .Build();
            FluentApi.Config(oracle_fsq);
            oracle_fsq.Ado.ExecuteNonQuery("drop table \"agc_app\" ");
            var ex = EnsureTables.ExistTable(oracle_fsq);
            Assert.IsFalse(ex);
            oracle_fsq.CodeFirst.SyncStructure<App>();
            ex = EnsureTables.ExistTable(oracle_fsq);
            Assert.IsTrue(ex);
        }

        [TestMethod()]
        public void ExistTablePostgreSqlTest()
        {
            //SqlServer
            string conn = "Host=127.0.0.1;Database=agile_config;Username=postgres;Password=dev@123";
            var postgresql_fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.PostgreSQL, conn)
                          .Build();
            FluentApi.Config(postgresql_fsq);
            postgresql_fsq.Ado.ExecuteNonQuery("drop table \"agc_app\" ");
            var ex = EnsureTables.ExistTable(postgresql_fsq);
            Assert.IsFalse(ex);
            postgresql_fsq.CodeFirst.SyncStructure<App>();
            ex = EnsureTables.ExistTable(postgresql_fsq);
            Assert.IsTrue(ex);
        }
    }
}