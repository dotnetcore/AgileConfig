using AgileConfig.Server.Data.Entity;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public class FreeSqlContext : DbContext
    {
        public DbSet<App> Apps { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<ServerNode> ServerNodes { get; set; }

        public DbSet<ModifyLog> ModifyLogs { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<SysLog> SysLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            //builder.UseFreeSql(FreeSqlContainer.FreeSqlInstance);
            //这里直接指定一个静态的 IFreeSql 对象即可，切勿重新 Build()
        }
    }
}
