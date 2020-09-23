using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using AgileConfig.Server.Data.Freesql;
using FreeSql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service.Tests.mysql
{
    [TestClass()]
    public class SysLogServiceTests
    {

        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        SysLogService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "Database=agile_config_test;Data Source=localhost;User Id=root;Password=dev@123;port=3306";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.MySql, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            freeSqlContext = new FreeSqlContext(fsq);

            service = new SysLogService(freeSqlContext);
            fsq.Delete<SysLog>().Where("1=1");

            Console.WriteLine("TestInitialize");
        }



        [TestCleanup]
        public void Clean()
        {
            freeSqlContext.Dispose();
            fsq.Dispose();
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

            var result = await service.AddSysLogAsync(source);
            Assert.IsTrue(result);

            var log = fsq.Select<SysLog>(new { 
                Id = source.Id
            }).ToOne();

            Assert.IsNotNull(log);

            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.AppId, log.AppId);
            Assert.AreEqual(source.LogType, log.LogType);
           // Assert.AreEqual(source.LogTime, log.LogTime);
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
            var result = await service.AddRangeAsync(new List<SysLog> {
                source, source1
            });
            Assert.IsTrue(result);

            var log = fsq.Select<SysLog>(new
            {
                Id = source.Id
            }).ToOne();
            Assert.IsNotNull(log);
            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.AppId, log.AppId);
            Assert.AreEqual(source.LogType, log.LogType);
          //  Assert.AreEqual(source.LogTime, log.LogTime);
            Assert.AreEqual(source.LogText, log.LogText);

            var log1 = fsq.Select<SysLog>(new
            {
                Id = source1.Id
            }).ToOne();
            Assert.IsNotNull(log1);
            Assert.AreEqual(source1.Id, log1.Id);
            Assert.AreEqual(source1.AppId, log1.AppId);
            Assert.AreEqual(source1.LogType, log1.LogType);
         //   Assert.AreEqual(source1.LogTime, log1.LogTime);
            Assert.AreEqual(source1.LogText, log1.LogText);
        }

       
        [TestMethod()]
        public async Task CountTest()
        {
            fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();

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
            var result = await service.AddRangeAsync(new List<SysLog> {
                source, source1
            });
            Assert.IsTrue(result);

           var count = await service.Count("001", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.AreEqual(1, count);

            var count1 = await service.Count("002", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
            Assert.AreEqual(0, count1);
        }

        [TestMethod()]
        public async Task SearchPageTest()
        {
            fsq.Delete<SysLog>().Where("1=1").ExecuteAffrows();

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
            var result = await service.AddRangeAsync(new List<SysLog> {
                source, source1
            });
            Assert.IsTrue(result);

            var page = await service.SearchPage("001", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1),1,0);
            Assert.AreEqual(1, page.Count);

            var page1 = await service.SearchPage("002", DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1), 1, 0);
            Assert.AreEqual(0, page1.Count);
        }
    }
}