using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using FreeSql;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Entity;
using System.Threading.Tasks;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.ServiceTests.sqlite
{
    [TestClass()]
    public class ConfigServiceTests : BasicTestService
    {
        IConfigService _service = null;
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
            _service = _serviceProvider.GetService<IConfigService>();
            _fsq.Delete<Config>().Where("1=1");
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

            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);
            var config = _fsq.Select<Config>(new
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
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), config.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.UpdateTime.Value.ToString("yyyyMMddHHmmss"), config.UpdateTime.Value.ToString("yyyyMMddHHmmss"));
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

            var result = await _service.AddAsync(source, "");
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

            var result1 = await _service.UpdateAsync(source, "");
            var config = _fsq.Select<Config>(new
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
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), config.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.UpdateTime.Value.ToString("yyyyMMddHHmmss"), config.UpdateTime.Value.ToString("yyyyMMddHHmmss"));
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

            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);

            var result1 = await _service.DeleteAsync(source, "");
            Assert.IsTrue(result1);

            var config = _fsq.Select<Config>(new
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

            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);

            var result1 = await _service.DeleteAsync(id, "");
            Assert.IsTrue(result1);

            var config = _fsq.Select<Config>(new
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

            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);

            var config = await _service.GetAsync(id, "");
            Assert.IsNotNull(config);

            Assert.AreEqual(source.Id, config.Id);
            Assert.AreEqual(source.Group, config.Group);
            Assert.AreEqual(source.Key, config.Key);
            Assert.AreEqual(source.Value, config.Value);
            Assert.AreEqual(source.Description, config.Description);
            Assert.AreEqual(source.AppId, config.AppId);
            Assert.AreEqual(source.CreateTime.ToString("yyyyMMddHHmmss"), config.CreateTime.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.UpdateTime.Value.ToString("yyyyMMddHHmmss"), config.UpdateTime.Value.ToString("yyyyMMddHHmmss"));
            Assert.AreEqual(source.Status, config.Status);
            Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
        }

        [TestMethod()]
        public async Task GetAllConfigsAsyncTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var env = "DEV";
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
            };
            var result = await _service.AddAsync(source, env);
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, env);
            Assert.IsTrue(result1);

            var configs = await _service.GetAllConfigsAsync(env);
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
        }

        [TestMethod()]
        public async Task GetByAppIdKeyTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            var env = "DEV";
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
            };
            var result = await _service.AddAsync(source, env);
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, env);
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, env);
            Assert.IsTrue(result2);

            var config = await _service.GetByAppIdKeyEnv("001", "g", "k", env);
            Assert.IsNotNull(config);

            var config1 = await _service.GetByAppIdKeyEnv("002", "g", "k", env);
            Assert.IsNull(config1);
        }

        [TestMethod()]
        public async Task GetByAppIdTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            var id = Guid.NewGuid().ToString();
            var env = "DEV";
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
            };
            var result = await _service.AddAsync(source, env);
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, env);
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, env);
            Assert.IsTrue(result2);

            var configs = await _service.GetByAppIdAsync("001", env);
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
        }

        [TestMethod()]
        public async Task SearchTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            var env = "DEV";
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                Env = env,
                OnlineStatus = OnlineStatus.Online,
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
                OnlineStatus = OnlineStatus.Online,
                Env = env,
            };
            var result = await _service.AddAsync(source, env);
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, env);
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, env);
            Assert.IsTrue(result2);

            var configs = await _service.Search("001", "", "", env);
            Assert.IsNotNull(configs);
            Assert.AreEqual(1, configs.Count);
            var configs1 = await _service.Search("", "o", "", env);
            Assert.IsNotNull(configs1);
            Assert.AreEqual(1, configs1.Count);
            var configs2 = await _service.Search("", "", "e", env);
            Assert.IsNotNull(configs2);
            Assert.AreEqual(1, configs2.Count);
        }

        [TestMethod()]
        public async Task CountEnabledConfigsAsyncTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

            string env = "DEV";
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
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
                OnlineStatus = OnlineStatus.Online,
                Env = env
            };
            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, "");
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, "");
            Assert.IsTrue(result2);

            var count = await _service.CountEnabledConfigsAsync();
            Assert.AreEqual(1, count);
        }

        [TestMethod()]
        public async Task AppPublishedConfigsMd5Test()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

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
            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, "");
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, "");
            Assert.IsTrue(result2);

            var md5 = await _service.AppPublishedConfigsMd5("001", "");
            Assert.IsNotNull(md5);
        }

        [TestMethod()]
        public void AppPublishedConfigsMd5CacheTest()
        {
        }

        [TestMethod()]
        public async Task GetPublishedConfigsByAppIdTest()
        {
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();

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
            var result = await _service.AddAsync(source, "");
            Assert.IsTrue(result);
            var result1 = await _service.AddAsync(source1, "");
            Assert.IsTrue(result1);
            var result2 = await _service.AddAsync(source2, "");
            Assert.IsTrue(result2);

            //var configs = await service.GetPublishedConfigsByAppId("001");
            //Assert.IsNotNull(configs);
            //Assert.AreEqual(1, configs.Count);
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

            var result = await _service.AddRangeAsync(new List<Config> {
                source,
                source1
            }, "");
            Assert.IsTrue(result);

            var config = _fsq.Select<Config>(new
            {
                Id = id
            }).ToOne();
            Assert.IsNotNull(config);
            var config1 = _fsq.Select<Config>(new
            {
                Id = id1
            }).ToOne();
            Assert.IsNotNull(config1);
        }

        [TestMethod()]
        public async Task GetPublishedConfigsByAppIdWithInheritanced_DictionaryTest()
        {
            _fsq.Delete<App>().Where("1=1").ExecuteAffrows();
            _fsq.Delete<Config>().Where("1=1").ExecuteAffrows();
            _fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();
            _fsq.Delete<ConfigPublished>().Where("1=1").ExecuteAffrows();
            _fsq.Delete<PublishTimeline>().Where("1=1").ExecuteAffrows();
            _fsq.Delete<PublishDetail>().Where("1=1").ExecuteAffrows();

            string env = "DEV";

            var app = new App();
            app.Id = "001";
            app.Name = "x";
            app.Enabled = true;
            app.CreateTime = DateTime.Now;
            app.UpdateTime = DateTime.Now;
            app.Type = AppType.PRIVATE; // app 001 私有
            var app1 = new App();
            app1.Id = "002";
            app1.Name = "x2";
            app1.Enabled = true;
            app1.CreateTime = DateTime.Now;
            app1.UpdateTime = DateTime.Now;
            app1.Type = AppType.Inheritance; // APP 002 公开

            var id = Guid.NewGuid().ToString();
            var source = new Config
            {
                AppId = "001",
                Id = id,
                Key = "k",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            var id1 = Guid.NewGuid().ToString();
            var source1 = new Config
            {
                AppId = "001",
                Id = id1,
                Key = "k1",
                Value = "v1",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            var source2 = new Config
            {
                AppId = "002",
                Id = Guid.NewGuid().ToString(),
                Key = "k2",
                Value = "v",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            var source3 = new Config
            {
                AppId = "002",
                Id = Guid.NewGuid().ToString(),
                Key = "k21",
                Value = "v2",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            var appref = new AppInheritanced();
            appref.AppId = app.Id;
            appref.InheritancedAppId = app1.Id; // app 001 继承 app 002
            appref.Sort = 1;
            appref.Id = Guid.NewGuid().ToString();

            _fsq.Insert(app).ExecuteAffrows();
            _fsq.Insert(app1).ExecuteAffrows();
            _fsq.Insert(appref).ExecuteAffrows();

            // 插入4个config，2个app 001，2个app 002
            _fsq.Insert(source).ExecuteAffrows();
            _fsq.Insert(source1).ExecuteAffrows();
            _fsq.Insert(source2).ExecuteAffrows();
            _fsq.Insert(source3).ExecuteAffrows();

            await _service.Publish(app1.Id, new string[] { }, "", "", env);
            await _service.Publish(app.Id, new string[] { }, "", "", env);

            var dict = await _service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, env);
            Assert.IsNotNull(dict);
            Assert.AreEqual(4, dict.Keys.Count);

            Assert.IsTrue(dict.ContainsKey(source.Key));
            Assert.IsTrue(dict.ContainsKey(source1.Key));
            Assert.IsTrue(dict.ContainsKey(source2.Key));
            Assert.IsTrue(dict.ContainsKey(source3.Key));

            Assert.IsTrue(dict[source.Key].Value == "v");
            Assert.IsTrue(dict[source1.Key].Value == "v1");
            Assert.IsTrue(dict[source2.Key].Value == "v");
            Assert.IsTrue(dict[source3.Key].Value == "v2");

            var source4 = new Config
            {
                AppId = "001",
                Id = Guid.NewGuid().ToString(),
                Key = "k4",
                Value = "v3",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            var source5 = new Config
            {
                AppId = "002",
                Id = Guid.NewGuid().ToString(),
                Key = "k4",
                Value = "v2",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            // 插入2个config，1个app 001，1个app 002，keyvalue相同，app 001 优先级高
            _fsq.Insert(source4).ExecuteAffrows();
            _fsq.Insert(source5).ExecuteAffrows();

            await _service.Publish(app1.Id, new string[] { }, "", "", env);
            await _service.Publish(app.Id, new string[] { }, "", "", env);

            dict = await _service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, env);
            Assert.IsNotNull(dict);
            Assert.AreEqual(5, dict.Keys.Count);

            var config1 = dict["k4"];
            Assert.AreEqual(source4.Value, config1.Value);

            var app2 = new App();
            app2.Id = "003";
            app2.Name = "x";
            app2.Enabled = true;
            app2.CreateTime = DateTime.Now;
            app2.UpdateTime = DateTime.Now;
            app2.Type = AppType.Inheritance;
            _fsq.Insert(app2).ExecuteAffrows();


            // 插入1个app 003
            _fsq.Delete<AppInheritanced>().Where("1=1").ExecuteAffrows();
            var appref1 = new AppInheritanced();
            appref1.AppId = app.Id;
            appref1.InheritancedAppId = app2.Id; // app 001 继承 app 003
            appref1.Sort = 2;
            appref1.Id = Guid.NewGuid().ToString();
            _fsq.Insert(appref1).ExecuteAffrows();

            var source6 = new Config
            {
                AppId = "003",
                Id = Guid.NewGuid().ToString(),
                Key = "k2",
                Value = "k4444",
                Description = "d",
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = ConfigStatus.Enabled,
                OnlineStatus = OnlineStatus.WaitPublish,
                Env = env
            };
            _fsq.Insert(source6).ExecuteAffrows();
            await _service.Publish(app2.Id, new string[] { }, "", "", env); // 发布app 003

            dict = await _service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, env);
            Assert.IsNotNull(dict);
            Assert.AreEqual(4, dict.Keys.Count);

            Assert.IsTrue(dict.ContainsKey(source6.Key));
            Assert.IsTrue(dict.ContainsKey(source4.Key));
            Assert.IsTrue(dict.ContainsKey(source1.Key));
            Assert.IsTrue(dict.ContainsKey(source.Key));

            Assert.IsTrue(dict[source6.Key].Value == "k4444");
            Assert.IsTrue(dict[source4.Key].Value == "v3");
            Assert.IsTrue(dict[source1.Key].Value == "v1");
            Assert.IsTrue(dict[source.Key].Value == "v");
        }
    }
}