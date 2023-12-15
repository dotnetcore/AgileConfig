using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Mongodb.ServiceTest;

public class ConfigServiceTests : DatabaseFixture
{
    private IConfigService service;

    [SetUp]
    public async Task TestInitialize()
    {
        var cache = new Mock<IMemoryCache>();
        var configuration = new Mock<IConfiguration>();
        var appService = new AppService(AppRepository, AppInheritancedRepository, ConfigRepository,
            ConfigPublishedRepository,
            UserAppAuthRepository, UserRepository);
        var settingService = new SettingService(SettingRepository, UserRepository, UserRoleRepository);
        var userService = new UserService(UserRepository, UserRoleRepository);
        service = new ConfigService(configuration.Object, cache.Object, appService, settingService, userService);
        
        await ConfigRepository.DeleteAsync(x => true);
    }

    [Test]
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
        var config = await ConfigRepository.FindAsync(id);

        Assert.IsTrue(result);
        Assert.IsNotNull(config);

        Assert.AreEqual(source.Id, config.Id);
        Assert.AreEqual(source.Group, config.Group);
        Assert.AreEqual(source.Key, config.Key);
        Assert.AreEqual(source.Value, config.Value);
        Assert.AreEqual(source.Description, config.Description);
        Assert.AreEqual(source.AppId, config.AppId);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), config.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), config.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Status, config.Status);
        Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
    }

    [Test]
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
        var config = await ConfigRepository.FindAsync(id);

        Assert.IsTrue(result1);
        Assert.IsNotNull(config);

        Assert.AreEqual(source.Id, config.Id);
        Assert.AreEqual(source.Group, config.Group);
        Assert.AreEqual(source.Key, config.Key);
        Assert.AreEqual(source.Value, config.Value);
        Assert.AreEqual(source.Description, config.Description);
        Assert.AreEqual(source.AppId, config.AppId);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), config.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), config.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Status, config.Status);
        Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
    }

    [Test]
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

        var config = await ConfigRepository.FindAsync(id);

        Assert.IsNull(config);
    }

    [Test]
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

        var config = await ConfigRepository.FindAsync(id);

        Assert.IsNull(config);
    }

    [Test]
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
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), config.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), config.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Status, config.Status);
        Assert.AreEqual(source.OnlineStatus, config.OnlineStatus);
    }

    [Test]
    public async Task GetAllConfigsAsyncTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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
            Env = "",
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
            Env = "",
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

    [Test]
    public async Task GetByAppIdKeyTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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
            Env = "env",
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
            Env = "env",
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
            Env = "env",
            OnlineStatus = OnlineStatus.Online
        };
        var result = await service.AddAsync(source, "env");
        Assert.IsTrue(result);
        var result1 = await service.AddAsync(source1, "env");
        Assert.IsTrue(result1);
        var result2 = await service.AddAsync(source2, "env");
        Assert.IsTrue(result2);

        var config = await service.GetByAppIdKeyEnv("001", "g", "k", "env");
        Assert.IsNotNull(config);

        var config1 = await service.GetByAppIdKeyEnv("002", "g", "k", "env");
        Assert.IsNull(config1);
    }

    [Test]
    public async Task GetByAppIdTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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
            Env = "",
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
            Env = "",
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
            Env = "",
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

    [Test]
    public async Task SearchTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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

    [Test]
    public async Task CountEnabledConfigsAsyncTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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

    [Test]
    public async Task AppPublishedConfigsMd5Test()
    {
        await ConfigRepository.DeleteAsync(x => true);

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

    [Test]
    public void AppPublishedConfigsMd5CacheTest()
    {
    }

    [Test]
    public async Task GetPublishedConfigsByAppIdTest()
    {
        await ConfigRepository.DeleteAsync(x => true);

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

    [Test]
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

        var config = await ConfigRepository.FindAsync(id);
        Assert.IsNotNull(config);
        var config1 = await ConfigRepository.FindAsync(id1);
        Assert.IsNotNull(config1);
    }

    [Test]
    public async Task GetPublishedConfigsByAppIdWithInheritanced_DictionaryTest()
    {
        await AppRepository.DeleteAsync(x => true);
        await ConfigRepository.DeleteAsync(x => true);
        await AppInheritancedRepository.DeleteAsync(x => true);

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

        await AppRepository.InsertAsync(app, app1);
        await ConfigRepository.InsertAsync(source, source1, source3);
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

        await ConfigRepository.InsertAsync(source4, source5);

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

        await AppInheritancedRepository.DeleteAsync(x => true);
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