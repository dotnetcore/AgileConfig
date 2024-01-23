using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Repository.Freesql;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    public class BasicTestService
    {
        protected ServiceProvider _serviceProvider;

        public virtual Task<Dictionary<string, string>> GetConfigurationData()
        {
            return null;
        }

        public virtual void ClearData()
        {
            var factory = new EnvFreeSqlFactory();
            var fsq = factory.Create("");

            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishDetail>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishTimeline>().Where("1=1").ExecuteAffrows();
            fsq.Delete<ConfigPublished>().Where("1=1").ExecuteAffrows();
            //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();
        }

        public BasicTestService()
        {
            Console.WriteLine("BasicTestService ctor.");
        }
    }
}
