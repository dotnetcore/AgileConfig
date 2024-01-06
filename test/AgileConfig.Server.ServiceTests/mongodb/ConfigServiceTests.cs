using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Mongodb;
using AgileConfig.Server.Data.Repository.Mongodb;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass]
public class ConfigServiceTests : Basic
{
    IConfigService service = null;


    [TestInitialize]
    public void TestInitialize()
    {
        service = GetService<IConfigService>();
    }


    [TestCleanup]
    public void Clean()
    {
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

        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var config = await ConfigRepository.GetAsync(id);


        Assert.IsTrue(result);
        Assert.IsNotNull(config);

        Assert.AreEqual(source.Id, config.Id);
        Assert.AreEqual(source.Group, config.Group);
        Assert.AreEqual(source.Key, config.Key);
        Assert.AreEqual(source.Value, config.Value);
        Assert.AreEqual(source.Description, config.Description);
        Assert.AreEqual(source.AppId, config.AppId);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
            config.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
            config.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
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

        var result = await service.AddAsync(source, "");
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

        var result1 = await service.UpdateAsync(source, "");
        var config = await ConfigRepository.GetAsync(id);

        Assert.IsTrue(result1);
        Assert.IsNotNull(config);

        Assert.AreEqual(source.Id, config.Id);
        Assert.AreEqual(source.Group, config.Group);
        Assert.AreEqual(source.Key, config.Key);
        Assert.AreEqual(source.Value, config.Value);
        Assert.AreEqual(source.Description, config.Description);
        Assert.AreEqual(source.AppId, config.AppId);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
            config.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
            config.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Status, config.Status);
        Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
    }

    [TestMethod()]
    public async Task UpdateAsyncListTest()
    {
        var ids = Enumerable.Range(0, 10).ToDictionary(x => x, _ => Guid.NewGuid().ToString());
        var list = Enumerable.Range(0, 10).Select(x => new Config
        {
            AppId = "001",
            Id = ids[x],
            Group = "g",
            Key = "k",
            Value = "v",
            Description = "d",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Status = ConfigStatus.Deleted,
            OnlineStatus = OnlineStatus.Online
        }).ToList();

        var result = await service.AddRangeAsync(list, "");
        Assert.IsTrue(result);

        foreach (var item in list)
        {
            var index = list.IndexOf(item);
            item.AppId = "1" + index;
            item.Group = "1" + index;
            item.Key = "1" + index;
            item.Value = "1" + index;
            item.Description = "1";
            item.CreateTime = DateTime.Now;
            item.UpdateTime = DateTime.Now;
            item.Status = ConfigStatus.Enabled;
            item.OnlineStatus = OnlineStatus.WaitPublish;
        }

        var result1 = await service.UpdateAsync(list, "");
        var idsValue = ids.Select(x => x.Value).ToList();
        var dbConfigs = await ConfigRepository.QueryAsync(x => idsValue.Contains(x.Id));

        Assert.IsTrue(result1);
        Assert.IsNotNull(dbConfigs);
        Assert.IsTrue(dbConfigs.Count > 0);

        foreach (var item in list)
        {
            var current = dbConfigs.FirstOrDefault(x => x.Id == item.Id);
            Assert.IsNotNull(current);

            Assert.AreEqual(item.Id, current.Id);
            Assert.AreEqual(item.Group, current.Group);
            Assert.AreEqual(item.Key, current.Key);
            Assert.AreEqual(item.Value, current.Value);
            Assert.AreEqual(item.Description, current.Description);
            Assert.AreEqual(item.AppId, current.AppId);
            Assert.AreEqual(item.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                current.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
            Assert.AreEqual(item.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"),
                current.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
            Assert.AreEqual(item.Status, current.Status);
            Assert.AreEqual(item.OnlineStatus, current.OnlineStatus);
        }
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

        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);

        var result1 = await service.DeleteAsync(source, "");
        Assert.IsTrue(result1);

        var config = await ConfigRepository.GetAsync(id);

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

        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);

        var result1 = await service.DeleteAsync(id, "");
        Assert.IsTrue(result1);

        var config = await ConfigRepository.GetAsync(id);

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

        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);

        var config = await service.GetAsync(id, "");
        Assert.IsNotNull(config);

        Assert.AreEqual(source.Id, config.Id);
        Assert.AreEqual(source.Group, config.Group);
        Assert.AreEqual(source.Key, config.Key);
        Assert.AreEqual(source.Value, config.Value);
        Assert.AreEqual(source.Description, config.Description);
        Assert.AreEqual(source.AppId, config.AppId);
        Assert.AreEqual(source.CreateTime, config.CreateTime);
        Assert.AreEqual(source.UpdateTime, config.UpdateTime);
        Assert.AreEqual(source.Status, config.Status);
        Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
    }

    [TestMethod()]
    public async Task GetAllConfigsAsyncTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);

        var configs = await service.GetAllConfigsAsync("");
        Assert.IsNotNull(configs);
        Assert.AreEqual(1, configs.Count);
    }

    [TestMethod()]
    public async Task GetByAppIdKeyTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
        Assert.IsTrue(result2);

        var config = await service.GetByAppIdKeyEnv("001", "g", "k", "env");
        Assert.IsNotNull(config);

        var config1 = await service.GetByAppIdKeyEnv("002", "g", "k", "env");
        Assert.IsNull(config1);
    }

    [TestMethod()]
    public async Task GetByAppIdTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
        Assert.IsTrue(result2);

        var configs = await service.GetByAppIdAsync("001", "");
        Assert.IsNotNull(configs);
        Assert.AreEqual(1, configs.Count);
    }

    [TestMethod()]
    public async Task SearchTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
        Assert.IsTrue(result2);

        var configs = await service.Search("001", "", "", "");
        Assert.IsNotNull(configs);
        Assert.AreEqual(1, configs.Count);
        var configs1 = await service.Search("", "o", "", "");
        Assert.IsNotNull(configs1);
        Assert.AreEqual(1, configs1.Count);
        var configs2 = await service.Search("", "", "e", "");
        Assert.IsNotNull(configs2);
        Assert.AreEqual(1, configs2.Count);
    }

    [TestMethod()]
    public async Task CountEnabledConfigsAsyncTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
        Assert.IsTrue(result2);

        var count = await service.CountEnabledConfigsAsync();
        Assert.AreEqual(1, count);
    }

    [TestMethod()]
    public async Task AppPublishedConfigsMd5Test()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
        Assert.IsTrue(result2);

        var md5 = await service.AppPublishedConfigsMd5("001", "");
        Assert.IsNotNull(md5);
    }

    [TestMethod()]
    public void AppPublishedConfigsMd5CacheTest()
    {
    }

    [TestMethod()]
    public async Task GetPublishedConfigsByAppIdTest()
    {
        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

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
        var result = await service.AddAsync(source, "");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "");
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

        var result = await service.AddRangeAsync(new List<Config>
        {
            source,
            source1
        }, "");
        Assert.IsTrue(result);

        var config = await ConfigRepository.GetAsync(id);
        Assert.IsNotNull(config);
        var config1 = await ConfigRepository.GetAsync(id);
        Assert.IsNotNull(config1);
    }

    [TestMethod()]
    public async Task GetPublishedConfigsByAppIdWithInheritanced_DictionaryTest()
    {
        var appAll = await AppRepository.AllAsync();
        await AppRepository.DeleteAsync(appAll);

        var all = await ConfigRepository.AllAsync();
        await ConfigRepository.DeleteAsync(all);

        var appInheritancedAll = await AppInheritancedRepository.AllAsync();
        await AppInheritancedRepository.DeleteAsync(appInheritancedAll);

        var app = new App();
        app.Id = "001";
        app.Name = "x";
        app.Enabled = true;
        app.CreateTime = DateTime.Now;
        app.UpdateTime = DateTime.Now;
        app.Type = AppType.PRIVATE;
        var app1 = new App();
        app1.Id = "002";
        app1.Name = "x";
        app1.Enabled = true;
        app1.CreateTime = DateTime.Now;
        app1.UpdateTime = DateTime.Now;
        app.Type = AppType.Inheritance;
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
            OnlineStatus = OnlineStatus.Online
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
            OnlineStatus = OnlineStatus.Online
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
            OnlineStatus = OnlineStatus.Online
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
            OnlineStatus = OnlineStatus.Online
        };
        var appref = new AppInheritanced();
        appref.AppId = app.Id;
        appref.InheritancedAppId = app1.Id;
        appref.Sort = 1;
        appref.Id = Guid.NewGuid().ToString();

        await AppRepository.InsertAsync(app);
        await AppRepository.InsertAsync(app1);
        await ConfigRepository.InsertAsync(source);
        await ConfigRepository.InsertAsync(source1);
        await ConfigRepository.InsertAsync(source2);
        await ConfigRepository.InsertAsync(source3);
        await AppInheritancedRepository.InsertAsync(appref);


        var dict = await service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, "");
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
            OnlineStatus = OnlineStatus.Online
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
            OnlineStatus = OnlineStatus.Online
        };

        await ConfigRepository.InsertAsync(source4);
        await ConfigRepository.InsertAsync(source5);

        dict = await service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, "");
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
        await AppRepository.InsertAsync(app2);
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
            OnlineStatus = OnlineStatus.Online
        };


        var appInheritancedAll2 = await AppInheritancedRepository.AllAsync();
        await AppInheritancedRepository.DeleteAsync(appInheritancedAll2);
        await ConfigRepository.InsertAsync(source6);
        await AppInheritancedRepository.InsertAsync(appref);


        var appref1 = new AppInheritanced();
        appref1.AppId = app.Id;
        appref1.InheritancedAppId = app2.Id;
        appref1.Sort = 2;
        appref1.Id = Guid.NewGuid().ToString();
        await AppInheritancedRepository.InsertAsync(appref1);
        dict = await service.GetPublishedConfigsByAppIdWithInheritanced_Dictionary(app.Id, "");
        Assert.IsNotNull(dict);
        Assert.AreEqual(5, dict.Keys.Count);

        config1 = dict["k2"];
        Assert.AreEqual(source6.Value, config1.Value);
    }
}