using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Common
{
    public interface ISqlContext
    {
        void InitTables();
    }

    public class SqlContext : DbContext, ISqlContext
    {
        public void InitTables()
        {
            this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbProvider = Configuration.Config["db:provider"];
            var conn = Configuration.Config["db:conn"];
            switch (dbProvider)
            {
                case "sqlserver":
                    optionsBuilder.UseSqlServer(conn);
                    break;
                case "mysql":
                    optionsBuilder.UseMySql(conn);
                    break;
                case "sqlite":
                    optionsBuilder.UseSqlite(conn);
                    break;
                default:
                    optionsBuilder.UseSqlite(conn);
                    break;
            }
#if DEBUG
            var lf = new LoggerFactory();
            lf.AddProvider(new EfLoggerProvider());
            optionsBuilder.UseLoggerFactory(lf);
#endif
        }
    }
}
