using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Service;
using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace AgileConfig.Server.Service.Tests.PostgreSQL
{
    [TestClass()]
    public class ConfigServiceTests
    {
        IFreeSql fsq = null;
        FreeSqlContext freeSqlContext;
        IConfigService service = null;

        [TestInitialize]
        public void TestInitialize()
        {
            string conn = "Host=127.0.0.1;Database=agile_config;Username=postgres;Password=dev@123";
            fsq = new FreeSqlBuilder()
                          .UseConnectionString(FreeSql.DataType.PostgreSQL, conn)
                          .UseAutoSyncStructure(true)
                          .Build();
            FluentApi.Config(fsq);
            freeSqlContext = new FreeSqlContext(fsq);

            var cache = new Mock<IMemoryCache>();
            var config = new Config();

            service = new ConfigService(freeSqlContext, cache.Object, new AppService(freeSqlContext));
            fsq.Delete<Config>().Where("1=1");

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
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };

           var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var config = fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();

            Assert.IsTrue(result);
            Assert.IsNotNull(config);

            Assert.AreEqual(source.Id, config.Id);
            Assert.AreEqual(source.Group, config.Group);
            Assert.AreEqual(source.Key, config.Key);
            Assert.AreEqual(source.Value, config.Value);
            Assert.AreEqual(source.Description, config.Description);
            Assert.AreEqual(source.AppId, config.AppId);
          //  Assert.AreEqual(source.CreateTime, config.CreateTime);
           // Assert.AreEqual(source.UpdateTime, config.UpdateTime);
            Assert.AreEqual(source.Status, config.Status);
            Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            source.AppId = "1";
            source.Group = "1";
            source.Key = "1";
            source.Value = "1";
            source.Description = "1";
            source.CreateTime = DateTime.Now;
            source.UpdateTime = DateTime.Now;
            source.Status = ConfigStatus.Enabled;
            source.OnlineStatus = OnlineStatus.WaitPublish;

            var result1 = await service.UpdateAsync(source);
            var config = fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();

            Assert.IsTrue(result1);
            Assert.IsNotNull(config);

            Assert.AreEqual(source.Id, config.Id);
            Assert.AreEqual(source.Group, config.Group);
            Assert.AreEqual(source.Key, config.Key);
            Assert.AreEqual(source.Value, config.Value);
            Assert.AreEqual(source.Description, config.Description);
            Assert.AreEqual(source.AppId, config.AppId);
         //   Assert.AreEqual(source.CreateTime, config.CreateTime);
         //   Assert.AreEqual(source.UpdateTime, config.UpdateTime);
            Assert.AreEqual(source.Status, config.Status);
            Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await service.DeleteAsync(source);
            Assert.IsTrue(result1);

            var config = fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();

            Assert.IsNull(config);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest1()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var result1 = await service.DeleteAsync(id);
            Assert.IsTrue(result1);

            var config = fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();

            Assert.IsNull(config);
        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };

            var result = await service.AddAsync(source);
            Assert.IsTrue(result);

            var config = await service.GetAsync(id);
            Assert.IsNotNull(config);

            Assert.AreEqual(source.Id, config.Id);
            Assert.AreEqual(source.Group, config.Group);
            Assert.AreEqual(source.Key, config.Key);
            Assert.AreEqual(source.Value, config.Value);
            Assert.AreEqual(source.Description, config.Description);
            Assert.AreEqual(source.AppId, config.AppId);
       //     Assert.AreEqual(source.CreateTime, config.CreateTime);
        //    Assert.AreEqual(source.UpdateTime, config.UpdateTime);
            Assert.AreEqual(source.Status, config.Status);
            Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
        }

        [TestMethod()]
        public async Task GetAllConfigsAsyncTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);

            var configs = await service.GetAllConfigsAsync();
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
        }

        [TestMethod()]
        public async Task GetByAppIdKeyTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var config = await service.GetByAppIdKey("001", "g", "k");
            Assert.IsNotNull(config);

            var config1 = await service.GetByAppIdKey("002", "g", "k");
            Assert.IsNull(config1);
        }

        [TestMethod()]
        public async Task GetByAppIdTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var configs = await service.GetByAppId("001");
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
        }

        [TestMethod()]
        public async Task SearchTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "group",
                Key = "key",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var configs = await service.Search("001", "", "");
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
            var configs1 = await service.Search("", "o", "");
            Assert.IsNotNull(configs1);
            Assert.AreEqual(1, configs1.Count);
            var configs2 = await service.Search("", "", "e");
            Assert.IsNotNull(configs2);
            Assert.AreEqual(1, configs2.Count);
        }

        [TestMethod()]
        public async Task CountEnabledConfigsAsyncTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "group",
                Key = "key",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var count = await service.CountEnabledConfigsAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod()]
        public async Task AppPublishedConfigsMd5Test()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "group",
                Key = "key",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var md5 = await service.AppPublishedConfigsMd5("001");
            Assert.IsNotNull(md5);
        }

        [TestMethod()]
        public void AppPublishedConfigsMd5CacheTest()
        {
        }

        [TestMethod()]
        public async Task GetPublishedConfigsByAppIdTest()
        {
            fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "group",
                Key = "key",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var id2 = Guid.NewGuid().ToString();
            var source2 = new Config
            {
                AppId = "002",
                Id = id2,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            var result = await service.AddAsync(source);
            Assert.IsTrue(result);
            var result1 = await service.AddAsync(source1);
            Assert.IsTrue(result1);
            var result2 = await service.AddAsync(source2);
            Assert.IsTrue(result2);

            var configs = await service.GetPublishedConfigsByAppId("001");
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
        }

        [TestMethod()]
        public async Task AddRangeAsyncTest()
        {
            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Group = "group",
                Key = "key",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.Online
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Group = "g",
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Deleted,
                OnlineStatus = OnlineStatus.Online
            };
            
            var result = await service.AddRangeAsync(new List<Config> {
                source,
                source1
            });
            Assert.IsTrue(result);

            var config = fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();
            Assert.IsNotNull(config);
            var config1 = fsq.Select<Config>(new
            {
                Id = id1
            }).ToOne();
            Assert.IsNotNull(config1);
        }
    }
}