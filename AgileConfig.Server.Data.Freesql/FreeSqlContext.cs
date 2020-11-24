using AgileConfig.Server.Data.Entity;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Freesql
{
    public class FreeSqlContext : DbContext
    {
        public FreeSqlContext()
        {
        }

        public FreeSqlContext(IFreeSql freeSql) : base(freeSql, null)
        {
        }

        public DbSet<App> Apps { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<ServerNode> ServerNodes { get; set; }

        public DbSet<ModifyLog> ModifyLogs { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<SysLog> SysLogs { get; set; }

        public DbSet<AppInheritanced> AppInheritanceds { get; set; }

    }
}
