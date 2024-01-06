﻿using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.Data.Repository.Freesql
{
    public class SysLogRepository : FreesqlRepository<SysLog, int>, ISysLogRepository
    {

        private readonly IFreeSql freeSql;

        public SysLogRepository(IFreeSql freeSql) : base(freeSql)
        {
            this.freeSql = freeSql;
        }
    }
}