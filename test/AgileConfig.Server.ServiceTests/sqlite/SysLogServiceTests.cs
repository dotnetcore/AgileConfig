using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Linq;
using AgileConfig.Server.Data.Abstraction;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.Data.Freesql;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    [TestClass()]
    public class SysLogServiceTests: BasicTestService
    {
        ISysLogService _syslogservice = null;
        public override Task<Dictionary<string, string>> GetConfigurationData()
        {
            return
                Task.FromResult(
                new Dictionary<string, string>
                {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" }
            });
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            Console.WriteLine("Try get configration data");
            var dict = await GetConfigurationData();

            var config = new ConfigurationBuilder()
                             .AddInMemoryCollection(dict)
                             .Build();
            Global.Config = config;

            ClearData();

            var cache = new Mock<IMemoryCache>();
            IServiceCollection services = new ServiceCollection();
            services.AddScoped(_ => cache.Object);
            services.AddSingleton<IConfiguration>(config);
            services.AddFreeSqlFactory();
            services.AddRepositories();
            services.AddBusinessServices();

            _serviceProvider = services.BuildServiceProvider();
            var systeminitializationService = _serviceProvider.GetService<ISystemInitializationService>();
            systeminitializationService.TryInitDefaultEnvironmentAsync();//初始化环境 DEV TEST STAGE PROD
            systeminitializationService.TryInitJwtSecret();//初始化 jwt secret


            _syslogservice = _serviceProvider.GetService<ISysLogService>();

            Console.WriteLine("Run TestInitialize");
        }



        [TestMethod()]
        public async Task AddSysLogAsyncTest()
        {
            var source = new SysLog
            {
                AppId = "001",
                LogType = SysLogType.Normal,
                LogTime = DateTime.Now,
                LogText = "123"
            };

            var result = await _syslogservice.AddSysLogAsync(source);
            Assert.IsTrue(result);

            var log = await _serviceProvider.GetService<ISysLogRepository>().GetAsync(source.Id);

            Assert.IsNotNull(log);

            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.AppId, log.AppId);
            Assert.AreEqual(source.LogType, log.LogType);
            Assert.AreEqual(source.LogTime.Value.ToString("yyyyMMddHHmmss"), log.LogTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LogText, log.LogText);
        }


        [TestMethod()]
        public async Task AddRangeAsyncTest()
        {
            var source = new SysLog
            {
                AppId = "001",
                LogType = SysLogType.Normal,
                LogTime = DateTime.Now,
                LogText = "123"
            };
            var source1 = new SysLog
            {
                AppId = "002",
                LogType = SysLogType.Warn,
                LogTime = DateTime.Now,
                LogText = "124"
            };
            var result = await _syslogservice.AddRangeAsync(new List<SysLog> {
                source, source1
            });
            Assert.IsTrue(result);

            var log = await _serviceProvider.GetService<ISysLogRepository>().GetAsync(source.Id);

            Assert.IsNotNull(log);
            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.AppId, log.AppId);
            Assert.AreEqual(source.LogType, log.LogType);
            Assert.AreEqual(source.LogTime.Value.ToString("yyyyMMddHHmmss"), log.LogTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LogText, log.LogText);

            var log1 = await _serviceProvider.GetService<ISysLogRepository>().GetAsync(source1.Id);

            Assert.IsNotNull(log1);
            Assert.AreEqual(source1.Id, log1.Id);
            Assert.AreEqual(source1.AppId, log1.AppId);
            Assert.AreEqual(source1.LogType, log1.LogType);
            Assert.AreEqual(source1.LogTime.Value.ToString("yyyyMMddHHmmss"), log1.LogTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source1.LogText, log1.LogText);
        }


        [TestMethod()]
        public async Task CountTest()
        {
            this.ClearData();
 
            var source = new SysLog
            {
                AppId = "001",
                LogType = SysLogType.Normal,
                LogTime = DateTime.Now,
                LogText = "123"
            };
            var source1 = new SysLog
            {
                AppId = "002",
                LogType = SysLogType.Warn,
                LogTime = DateTime.Now,
                LogText = "124"
            };
            var result = await _syslogservice.AddRangeAsync(new List<SysLog> {
                source, source1
            });
            Assert.IsTrue(result);

            var count = await _syslogservice.Count("001", SysLogType.Normal, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.AreEqual(1, count);

            var count1 = await _syslogservice.Count("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
            Assert.AreEqual(0, count1);
        }

        [TestMethod()]
        public async Task SearchPageTest()
        {
            this.ClearData();

            var source = new SysLog
            {
                AppId = "001",
                LogType = SysLogType.Normal,
                LogTime = DateTime.Now,
                LogText = "123"
            };
            var source1 = new SysLog
            {
                AppId = "002",
                LogType = SysLogType.Warn,
                LogTime = DateTime.Now,
                LogText = "124"
            };
            var result = await _syslogservice.AddSysLogAsync(source);
            Assert.IsTrue(result);
            result = await _syslogservice.AddSysLogAsync(source1);
            Assert.IsTrue(result);

            var log = await _serviceProvider.GetService<ISysLogRepository>().GetAsync(source.Id);
            Assert.IsNotNull(log);

            var log1 = await _serviceProvider.GetService<ISysLogRepository>().GetAsync(source1.Id);
            Assert.IsNotNull(log1);

            var page = await _syslogservice.SearchPage("001", SysLogType.Normal, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), 1, 1);
            Assert.AreEqual(1, page.Count);

            var page1 = await _syslogservice.SearchPage("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1), 1, 1);
            Assert.AreEqual(0, page1.Count);
        }
    }
}