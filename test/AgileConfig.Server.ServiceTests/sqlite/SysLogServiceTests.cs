using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver.Linq;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    [TestClass()]
    public class SysLogServiceTests: BasicTestService
    {
        ISysLogService _syslogservice = null;
        public override Dictionary<string, string> GetConfigurationData()
        {
            return
                new Dictionary<string, string>
                {
                {"db:provider","sqlite" },
                {"db:conn","Data Source=agile_config.db" }
            };
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _syslogservice = _serviceProvider.GetService<ISysLogService>();
            _fsq.Delete<SysLog>().Where("1=1");
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

            var log = _fsq.Select<SysLog>(new
            {
                source.Id
            }).ToOne();

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

            var log = _fsq.Select<SysLog>(new
            {
                source.Id
            }).ToOne();
            Assert.IsNotNull(log);
            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.AppId, log.AppId);
            Assert.AreEqual(source.LogType, log.LogType);
            Assert.AreEqual(source.LogTime.Value.ToString("yyyyMMddHHmmss"), log.LogTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.LogText, log.LogText);

            var log1 = _fsq.Select<SysLog>(new
            {
                source1.Id
            }).ToOne();
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
            _fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();

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
            _fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();

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

            var page = await _syslogservice.SearchPage("001", SysLogType.Normal, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), 1, 0);
            Assert.AreEqual(1, page.Count);

            var page1 = await _syslogservice.SearchPage("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1), 1, 0);
            Assert.AreEqual(0, page1.Count);
        }
    }
}