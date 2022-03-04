﻿using AgileConfig.Server.Data.Entity;
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

        public IFreeSql Freesql
        {
            get;
        }

        public FreeSqlContext(IFreeSql freeSql) : base(freeSql, null)
        {
            Freesql = freeSql;
        }

        public DbSet<App> Apps { get; set; }

        public DbSet<Config> Configs { get; set; }

        public DbSet<ServerNode> ServerNodes { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<SysLog> SysLogs { get; set; }

        public DbSet<AppInheritanced> AppInheritanceds { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<UserAppAuth> UserAppAuths { get; set; }

        public DbSet<ConfigPublished> ConfigPublished { get; set; }
        public DbSet<PublishDetail> PublishDetail { get; set; }
        public DbSet<PublishTimeline> PublishTimeline { get; set; }
        public DbSet<ServiceInfo> ServiceInfo { get; set; }

    }
}
