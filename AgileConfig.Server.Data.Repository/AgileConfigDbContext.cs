using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Repository
{
    public class AgileConfigDbContext : SqlContext
    {
        public DbSet<App> Apps { get; set; }

        public DbSet<Config> Configs { get; set; }
    }
}
