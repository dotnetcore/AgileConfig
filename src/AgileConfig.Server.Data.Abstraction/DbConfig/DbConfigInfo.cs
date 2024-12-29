using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Data.Abstraction.DbProvider
{
    public class DbConfigInfo : IDbConfigInfo
    {
        public DbConfigInfo(string env, string provider, string conn)
        {
            this.Env = env;
            this.Provider = provider;
            this.ConnectionString = conn;
        }
        public string Env { get;  }

        public string Provider { get;  }

        public string ConnectionString { get; }
    }
}
