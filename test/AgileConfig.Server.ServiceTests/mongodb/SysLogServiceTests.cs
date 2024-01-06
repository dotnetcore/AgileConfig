using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.ServiceTests.mongodb;

[TestClass()]
public class SysLogServiceTests : Basic
{
    ISysLogService service = null;

    [TestInitialize]
    public void TestInitialize()
    {
        service = GetService<ISysLogService>();
    }


    [TestCleanup]
    public void Clean()
    {
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

        var log = await SysLogRepository.GetAsync(source.Id);

        Assert.IsNotNull(log);

        Assert.AreEqual(source.Id, log.Id);
        Assert.AreEqual(source.AppId, log.AppId);
        Assert.AreEqual(source.LogType, log.LogType);
        Assert.AreEqual(source.LogTime, log.LogTime);
        Assert.AreEqual(source.LogText, log.LogText);
    }

    [TestMethod()]
    public async Task AddSysLogTransactionAsyncTest()
    {
        var uowAccessor = GetService<Func<string, IUow>>();
        var uow = uowAccessor("");

        var id = Guid.NewGuid().ToString();
        var source = new SysLog
        {
            Id = id,
            AppId = "001",
            LogType = SysLogType.Normal,
            LogTime = DateTime.Now,
            LogText = "123"
        };

        
        try
        {
            SysLogRepository.Uow = uow;
            uow?.Begin();
            await SysLogRepository.InsertAsync(source);
            throw new Exception();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            uow?.Rollback();
        }
        finally
        {
            uow?.Dispose();
        }

        var log = await SysLogRepository.GetAsync(id);

        Assert.IsNull(log);
        
        
        var uowAccessor2 = GetService<Func<string, IUow>>();
        var uow2 = uowAccessor2("");
        SysLogRepository.Uow = uow2;
        uow2.Begin();
        await SysLogRepository.InsertAsync(source);
        await uow2.SaveChangesAsync();
        var log2 = await SysLogRepository.GetAsync(id);
        uow2.Dispose();

        Assert.AreEqual(source.Id, log2.Id);
        Assert.AreEqual(source.AppId, log2.AppId);
        Assert.AreEqual(source.LogType, log2.LogType);
        Assert.AreEqual(source.LogTime?.ToString("yyyy/MM/dd HH:mm:ss"), log2.LogTime?.ToString("yyyy/MM/dd HH:mm:ss"));
        Assert.AreEqual(source.LogText, log2.LogText);
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
        var result = await service.AddRangeAsync(new List<SysLog>
        {
            source, source1
        });
        Assert.IsTrue(result);

        var log = await SysLogRepository.GetAsync(source.Id);
        Assert.IsNotNull(log);
        Assert.AreEqual(source.Id, log.Id);
        Assert.AreEqual(source.AppId, log.AppId);
        Assert.AreEqual(source.LogType, log.LogType);
        Assert.AreEqual(source.LogTime, log.LogTime);
        Assert.AreEqual(source.LogText, log.LogText);

        var log1 = await SysLogRepository.GetAsync(source.Id);
        Assert.IsNotNull(log1);
        Assert.AreEqual(source1.Id, log1.Id);
        Assert.AreEqual(source1.AppId, log1.AppId);
        Assert.AreEqual(source1.LogType, log1.LogType);
        Assert.AreEqual(source1.LogTime, log1.LogTime);
        Assert.AreEqual(source1.LogText, log1.LogText);
    }


    [TestMethod()]
    public async Task CountTest()
    {
        var all = await SysLogRepository.AllAsync();
        await SysLogRepository.DeleteAsync(all);

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

    [TestMethod()]
    public async Task SearchPageTest()
    {
        var all = await SysLogRepository.AllAsync();
        await SysLogRepository.DeleteAsync(all);

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
            1, 1);
        Assert.AreEqual(1, page.Count);

        var page1 = await service.SearchPage("002", SysLogType.Warn, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-1),
            0, 0);
        Assert.AreEqual(0, page1.Count);
    }
}