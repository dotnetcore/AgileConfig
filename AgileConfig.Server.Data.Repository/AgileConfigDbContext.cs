using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AgileConfig.Server.Data.Repository
{
    public class AgileConfigDbContext : SqlContext
    {
        public DbSet<App> Apps { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<ServerNode> ServerNodes { get; set; }

        public DbSet<ModifyLog> ModifyLogs { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<SysLog> SysLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dbProvider = Configuration.Config["db:provider"];
            FixColumnsDataType<App>(modelBuilder, dbProvider);
            FixColumnsDataType<Config>(modelBuilder, dbProvider);
            FixColumnsDataType<ModifyLog>(modelBuilder, dbProvider);
            FixColumnsDataType<ServerNode>(modelBuilder, dbProvider);
            FixColumnsDataType<Setting>(modelBuilder, dbProvider);
            FixColumnsDataType<SysLog>(modelBuilder, dbProvider);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// 修复字段跟数据库字段对应关系，因为不同的数据库有些差异
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelBuilder"></param>
        /// <param name="dbProvider"></param>
        private void FixColumnsDataType<T>(ModelBuilder modelBuilder, string dbProvider) where T : class
        {
            var props = typeof(T).GetProperties();
            foreach (var item in props)
            {
                if (item.PropertyType == typeof(string))
                {
                    modelBuilder.Entity<T>(builer =>
                    {
                        var att = item.GetCustomAttributes().FirstOrDefault(att => att.GetType() == typeof(ColumnAttribute));
                        if (att != null)
                        {
                            var columnAtt = att as ColumnAttribute;
                            var type = columnAtt.TypeName;
                            if (!string.IsNullOrEmpty(type))
                            {
                                if (dbProvider == "npgsql")
                                {
                                    //npgsql没有nvarcahr
                                    builer.Property(item.Name).HasColumnType(type.Replace("nvarchar", "varchar"));
                                }
                                //if sqlserver
                                //if sqlite
                                //if mysql
                            }
                        }
                    });
                }
            }
        }
    }
}
