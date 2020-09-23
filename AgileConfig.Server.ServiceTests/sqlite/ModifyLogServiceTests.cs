using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using FreeSql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service.Tests
{
    [TestClass()]
    public class ModifyLogServiceTests
    {
        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        ModifyLogService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "Data Source=agile_config.db";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.Sqlite, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            freeSqlContext = new FreeSqlContext(fsq);

            service = new ModifyLogService(freeSqlContext);
            fsq.Delete<ModifyLog>().Where("1=1");

            Console.WriteLine("TestInitialize");
        }



        [TestCleanup]
        public void Clean()
        {
            freeSqlContext.Dispose();
            fsq.Dispose();
        }

        [TestMethod()]
        public async Task AddAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new ModifyLog
            {
                Id = id,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var log = fsq.Select<ModifyLog>(new { 
                Id = id
            }).ToOne();

            Assert.IsNotNull(log);

            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.ConfigId, log.ConfigId);
            Assert.AreEqual(source.Group, log.Group);
            Assert.AreEqual(source.Key, log.Key);
            Assert.AreEqual(source.Value, log.Value);
            Assert.AreEqual(source.ModifyTime, log.ModifyTime);
        }

        [TestMethod()]
        public async Task AddRangAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new ModifyLog
            {
                Id = id,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new ModifyLog
            {
                Id = id1,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };
            var result = await service.AddRangAsync(new List<ModifyLog> { 
                source,
                source1
            });
            Assert.IsTrue(result);

            var log = fsq.Select<ModifyLog>(new
            {
                Id = id
            }).ToOne();

            Assert.IsNotNull(log);

            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.ConfigId, log.ConfigId);
            Assert.AreEqual(source.Group, log.Group);
            Assert.AreEqual(source.Key, log.Key);
            Assert.AreEqual(source.Value, log.Value);
            Assert.AreEqual(source.ModifyTime, log.ModifyTime);

            var log1 = fsq.Select<ModifyLog>(new
            {
                Id = id1
            }).ToOne();

            Assert.IsNotNull(log1);

            Assert.AreEqual(source1.Id, log1.Id);
            Assert.AreEqual(source1.ConfigId, log1.ConfigId);
            Assert.AreEqual(source1.Group, log1.Group);
            Assert.AreEqual(source1.Key, log1.Key);
            Assert.AreEqual(source1.Value, log1.Value);
            Assert.AreEqual(source1.ModifyTime, log1.ModifyTime);
        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new ModifyLog
            {
                Id = id,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var log = await service.GetAsync(id);

            Assert.IsNotNull(log);

            Assert.AreEqual(source.Id, log.Id);
            Assert.AreEqual(source.ConfigId, log.ConfigId);
            Assert.AreEqual(source.Group, log.Group);
            Assert.AreEqual(source.Key, log.Key);
            Assert.AreEqual(source.Value, log.Value);
            Assert.AreEqual(source.ModifyTime, log.ModifyTime);
        }

        [TestMethod()]
        public async Task SearchTest()
        {
            fsq.Delete<ModifyLog>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new ModifyLog
            {
                Id = id,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new ModifyLog
            {
                Id = id1,
                ConfigId = "123",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new ModifyLog
            {
                Id = id2,
                ConfigId = "1234",
                Group = "0",
                Key = "k",
                Value = "v",
                ModifyTime = DateTime.Now
            };
            var result = await service.AddRangAsync(new List<ModifyLog> {
                source,
                source1,
                source2
            });
            Assert.IsTrue(result);

            var configs = await service.Search("123");
            Assert.IsNotNull(configs);

            Assert.AreEqual(2, configs.Count);

        }
    }
}