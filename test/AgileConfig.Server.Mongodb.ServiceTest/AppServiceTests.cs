namespace AgileConfig.Server.Mongodb.ServiceTest;

[TestFixture]
public class AppServiceTests : DatabaseFixture
{
    private IAppService service;

    [SetUp]
    public async Task TestInitialize()
    {
        service = new AppService(AppRepository, AppInheritancedRepository, ConfigRepository, ConfigPublishedRepository,
            UserAppAuthRepository, UserRepository);
        await AppRepository.DeleteAsync(x => true);
    }
    
    [Test]
    public async Task AddAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        var app = await AppRepository.FindAsync(id);

        Assert.IsTrue(result);
        Assert.IsNotNull(app);

        Assert.AreEqual(source.Id, app.Id);
        Assert.AreEqual(source.Name, app.Name);
        Assert.AreEqual(source.Secret, app.Secret);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), app.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), app.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Enabled, app.Enabled);
    }

    [Test]
    public async Task DeleteAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var delResult = await service.DeleteAsync(source);
        Assert.IsTrue(delResult);

        var app = await AppRepository.FindAsync(id);
        Assert.IsNull(app);
    }

    [Test]
    public async Task DeleteAsyncTest1()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var delResult = await service.DeleteAsync(id);
        Assert.IsTrue(delResult);

        var app = await AppRepository.FindAsync(id);
        Assert.IsNull(app);
    }

    [Test]
    public async Task GetAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var app = await service.GetAsync(id);
        Assert.IsNotNull(app);

        Assert.AreEqual(source.Id, app.Id);
        Assert.AreEqual(source.Name, app.Name);
        Assert.AreEqual(source.Secret, app.Secret);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), app.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), app.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Enabled, app.Enabled);
    }

    [Test]
    public async Task GetAllAppsAsyncTest()
    {
        await AppRepository.DeleteAsync(x => true);
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);
        var id1 = Guid.NewGuid().ToString();
        var source1 = new Data.Entity.App
        {
            Id = id1,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result1 = await service.AddAsync(source1);
        Assert.IsTrue(result1);

        var apps = await service.GetAllAppsAsync();
        Assert.IsNotNull(apps);
        Assert.AreEqual(2, apps.Count);
    }

    [Test]
    public async Task UpdateAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        source.Name = "new name";
        source.Secret = "new sec";
        source.CreateTime = DateTime.Now.AddDays(1);
        source.UpdateTime = DateTime.Now.AddDays(1);
        source.Enabled = false;

        var result1 = await service.UpdateAsync(source);
        Assert.IsTrue(result1);

        var app = await AppRepository.FindAsync(id);

        Assert.AreEqual(source.Id, app.Id);
        Assert.AreEqual(source.Name, app.Name);
        Assert.AreEqual(source.Secret, app.Secret);
        Assert.AreEqual(source.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"), app.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"), app.UpdateTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.Enabled, app.Enabled);
    }

    [Test]
    public async Task CountEnabledAppsAsyncTest()
    {
        await AppRepository.DeleteAsync(x => true);
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true
        };
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);
        var id1 = Guid.NewGuid().ToString();
        var source1 = new Data.Entity.App
        {
            Id = id1,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = false
        };
        var result1 = await service.AddAsync(source1);
        Assert.IsTrue(result1);

        var count = await service.CountEnabledAppsAsync();
        Assert.AreEqual(1, count);
    }

    [Test]
    public async Task GetAllInheritancedAppsAsyncTest()
    {
        await AppRepository.DeleteAsync(x => true);
        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.PRIVATE
        };
        var source1 = new Data.Entity.App
        {
            Id = Guid.NewGuid().ToString(),
            Name = "xxx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.PRIVATE
        };
        var source2 = new Data.Entity.App
        {
            Id = Guid.NewGuid().ToString(),
            Name = "xxxx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.Inheritance
        };
        var source3 = new Data.Entity.App
        {
            Id = Guid.NewGuid().ToString(),
            Name = "xxxx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = false,
            Type = AppType.Inheritance
        };
        var result = await service.AddAsync(source);
        await service.AddAsync(source1);
        await service.AddAsync(source2);
        await service.AddAsync(source3);

        Assert.IsTrue(result);

        var apps = await service.GetAllInheritancedAppsAsync();

        Assert.AreEqual(2, apps.Count);
    }

    [Test]
    public async Task GetInheritancedAppsAsyncTest()
    {
        await AppRepository.DeleteAsync(x => true);
        await AppInheritancedRepository.DeleteAsync(x => true);

        var id = Guid.NewGuid().ToString();
        var source = new Data.Entity.App
        {
            Id = id,
            Name = "xx",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.PRIVATE
        };
        var source1 = new Data.Entity.App
        {
            Id = Guid.NewGuid().ToString(),
            Name = "xx1",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.Inheritance
        };
        var source2 = new Data.Entity.App
        {
            Id = Guid.NewGuid().ToString(),
            Name = "xx2",
            Secret = "sec",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            Enabled = true,
            Type = AppType.Inheritance
        };
        //
        var appInher = new AppInheritanced();
        appInher.Id = Guid.NewGuid().ToString();
        appInher.AppId = source.Id;
        appInher.InheritancedAppId = source1.Id;
        appInher.Sort = 1;
        var appInher1 = new AppInheritanced();
        appInher1.Id = Guid.NewGuid().ToString();
        appInher1.AppId = source.Id;
        appInher1.InheritancedAppId = source2.Id;
        appInher1.Sort = 2;

        var result = await service.AddAsync(source);
        await service.AddAsync(source1);
        await service.AddAsync(source2);

        await AppInheritancedRepository.InsertAsync(appInher);
        await AppInheritancedRepository.InsertAsync(appInher1);

        Assert.IsTrue(result);

        var apps = await service.GetInheritancedAppsAsync(source.Id);

        Assert.AreEqual(2, apps.Count);
    }
}