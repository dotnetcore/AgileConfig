using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService
{
    public interface IServiceHealthCheckService
    {
        Task StartCheckAsync();
    }
}
