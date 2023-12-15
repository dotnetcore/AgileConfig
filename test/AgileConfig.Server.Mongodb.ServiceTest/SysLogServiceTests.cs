namespace AgileConfig.Server.Mongodb.ServiceTest;

public class SysLogServiceTests : DatabaseFixture
{
    private ISysLogService service;

    [SetUp]
    public async Task TestInitialize()
    {
        service = new SysLogService(SysLogRepository);
        await SysLogRepository.DeleteAsync(x => true);
        Console.WriteLine("TestInitialize");
    }

    [Test]
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

        var log = await SysLogRepository.FindAsync(source.Id);

        Assert.IsNotNull(log);

        Assert.AreEqual(source.Id, log.Id);
        Assert.AreEqual(source.AppId, log.AppId);
        Assert.AreEqual(source.LogType, log.LogType);
        //   Assert.AreEqual(source.LogTime, log.LogTime);
        Assert.AreEqual(source.LogText, log.LogText);
    }


    [Test]
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
        var result = await service.AddRangeAsync(new List<SysLog>
        {
            source, source1
        });
        Assert.IsTrue(result);

        var log = await SysLogRepository.FindAsync(source.Id);
        Assert.IsNotNull(log);
        Assert.AreEqual(source.Id, log.Id);
        Assert.AreEqual(source.AppId, log.AppId);
        Assert.AreEqual(source.LogType, log.LogType);
        //  Assert.AreEqual(source.LogTime, log.LogTime);
        Assert.AreEqual(source.LogText, log.LogText);

        var log1 = await SysLogRepository.FindAsync(source1.Id);
        Assert.IsNotNull(log1);
        Assert.AreEqual(source1.Id, log1.Id);
        Assert.AreEqual(source1.AppId, log1.AppId);
        Assert.AreEqual(source1.LogType, log1.LogType);
        //  Assert.AreEqual(source1.LogTime, log1.LogTime);
        Assert.AreEqual(source1.LogText, log1.LogText);
    }


    [Test]
    public async Task CountTest()
    {
        await SysLogRepository.DeleteAsync(x => true);

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
        var result = await service.AddRangeAsync(new List<SysLog>
        {
            source, source1
        });
        Assert.IsTrue(result);

        var count = await service.Count("001", SysLogType.Normal, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
        Assert.AreEqual(1, count);

        var count1 = await service.Count("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1));
        Assert.AreEqual(0, count1);
    }

    [Test]
    public async Task SearchPageTest()
    {
        await SysLogRepository.DeleteAsync(x => true);

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
        var result = await service.AddRangeAsync(new List<SysLog>
        {
            source, source1
        });
        Assert.IsTrue(result);

        var page = await service.SearchPage("001", SysLogType.Normal, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1),
            1, 0);
        Assert.AreEqual(1, page.Count);

        var page1 = await service.SearchPage("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1),
            1, 0);
        Assert.AreEqual(0, page1.Count);
    }
}