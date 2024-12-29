using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    public class BasicTestService
    {
        protected ServiceProvider GlobalServiceProvider { get; set; }
        
        public virtual Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
              null;
        }

        public IFreeSql GetFreeSql()
        {
            var dict = this.GetConfigurationData().Result;

            var config = new ConfigurationBuilder()
                       .AddInMemoryCollection(dict)
                       .Build();

            var myfreesql = new MyFreeSQL(new DbConfigInfoFactory(config));
            var factory = new EnvFreeSqlFactory(myfreesql);
            var fsq = factory.Create("");

            return fsq;
        }

        public virtual void ClearData()
        {
            var fsq = GetFreeSql();

            fsq.Delete<ServerNode>().Where("1=1").ExecuteAffrows();
            fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishDetail>().Where("1=1").ExecuteAffrows();
            fsq.Delete<PublishTimeline>().Where("1=1").ExecuteAffrows();
            fsq.Delete<ConfigPublished>().Where("1=1").ExecuteAffrows();
            //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
            fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();

            fsq.Dispose();
        }

        public BasicTestService()
        {
            Console.WriteLine("BasicTestService ctor.");
        }
    }
}
