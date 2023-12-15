namespace AgileConfig.Server.Mongodb.ServiceTest;

public class SettingServiceTests : DatabaseFixture
{
    private ISettingService service;

    [SetUp]
    public async Task TestInitialize()
    {
        service = new SettingService(SettingRepository, UserRepository, UserRoleRepository);
        await SettingRepository.DeleteAsync(x => true);
        Console.WriteLine("TestInitialize");
    }

    [Test]
    public async Task AddAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var setting = await SettingRepository.FindAsync(id);

        Assert.IsNotNull(setting);

        Assert.AreEqual(source.Id, setting.Id);
        Assert.AreEqual(source.Value, setting.Value);
    }

    [Test]
    public async Task DeleteAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        result = await service.DeleteAsync(source);
        Assert.IsTrue(result);

        var setting = await SettingRepository.FindAsync(id);

        Assert.IsNull(setting);
    }

    [Test]
    public async Task DeleteAsyncTest1()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        result = await service.DeleteAsync(id);
        Assert.IsTrue(result);

        var setting = await SettingRepository.FindAsync(id);

        Assert.IsNull(setting);
    }

    [Test]
    public async Task GetAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        var setting = await service.GetAsync(id);

        Assert.IsNotNull(setting);

        Assert.AreEqual(source.Id, setting.Id);
        Assert.AreEqual(source.Value, setting.Value);
    }

    [Test]
    public async Task GetAllSettingsAsyncTest()
    {
        await SettingRepository.DeleteAsync(x => true);
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);
        var id1 = Guid.NewGuid().ToString();
        var source1 = new Setting();
        source1.Id = id1;
        source1.Value = "123";
        source1.CreateTime = DateTime.Now;
        var result1 = await service.AddAsync(source1);
        Assert.IsTrue(result1);

        var settings = await service.GetAllSettingsAsync();

        Assert.IsNotNull(settings);

        Assert.AreEqual(2, settings.Count);
    }

    [Test]
    public async Task UpdateAsyncTest()
    {
        var id = Guid.NewGuid().ToString();
        var source = new Setting();
        source.Id = id;
        source.Value = "123";
        source.CreateTime = DateTime.Now;
        var result = await service.AddAsync(source);
        Assert.IsTrue(result);

        source.CreateTime = DateTime.Now;
        source.Value = "321";
        var result1 = await service.UpdateAsync(source);
        Assert.IsTrue(result1);

        var setting = await service.GetAsync(id);
        Assert.IsNotNull(setting);

        Assert.AreEqual(source.Id, setting.Id);
        Assert.AreEqual(source.Value, setting.Value);
    }

    [Test]
    public async Task SetAdminPasswordTest()
    {
        //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
        //var result = await service.SetSuperAdminPassword("123456");
        //Assert.IsTrue(result);
        //var list = fsq.Select<Setting>().Where("1=1").ToList();
        //Assert.IsNotNull(list);
        //Assert.AreEqual(2, list.Count);

        //var pass = list.FirstOrDefault(s => s.Id == service.SuperAdminPasswordSettingKey);
        //Assert.IsNotNull(pass);
        //var salt = list.FirstOrDefault(s => s.Id == service.AdminPasswordHashSaltKey);
        //Assert.IsNotNull(salt);
    }

    [Test]
    public async Task HasAdminPasswordTest()
    {
        //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
        //var result = await service.SetSuperAdminPassword("123456");
        //Assert.IsTrue(result);

        //var has = await service.HasSuperAdminPassword();
        //Assert.IsTrue(has);
        //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
        //has = await service.HasSuperAdminPassword();
        //Assert.IsFalse(has);
    }

    [Test]
    public async Task ValidateAdminPasswordTest()
    {
        //fsq.Delete<Setting>().Where("1=1").ExecuteAffrows();
        //var result = await service.SetSuperAdminPassword("123456");
        //Assert.IsTrue(result);

        //var v = await service.ValidateAdminPassword("123456");
        //Assert.IsTrue(v);
        //v = await service.ValidateAdminPassword("1234561");
        //Assert.IsFalse(v);
    }
}