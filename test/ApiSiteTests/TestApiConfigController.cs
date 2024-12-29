using AgileConfig.Server.Apisite.Controllers.api;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Models;

namespace ApiSiteTests;

[TestClass]
public class TestApiConfigController
{
    [TestMethod]
    public async Task TestGet()
    {
        App newApp()
        {
            return new App()
            {
                Enabled = true
            };
        }

        var appService = new Mock<IAppService>();
        appService.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(newApp);

        List<Config> newConfigs()
        {
            var list = new List<Config>();
            list.Add(new Config
            {
                Id = "001",
                Key = "key1",
                Value = "val1",
                Group = ""
            });
            list.Add(new Config
            {
                Id = "002",
                Key = "key2",
                Value = "val2",
                Group = "group1",
                AppId = "001",
                Status = ConfigStatus.Enabled
            });

            return list;
        }
        var configService = new Mock<IConfigService>();
        //configService.Setup(s => s.GetPublishedConfigsAsync("001"))
        //    .ReturnsAsync(newConfigs);
        configService.Setup(s => s.GetPublishedConfigsByAppIdWithInheritanced(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(newConfigs);

        IMemoryCache memoryCache = null;
        var userSErvice = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var meterService = new Mock<IMeterService>();

        var ctrl = new ConfigController(
            configService.Object,
            appService.Object,
            memoryCache,
            meterService.Object,
            new AgileConfig.Server.Apisite.Controllers.ConfigController(configService.Object, appService.Object, userSErvice.Object, eventBus.Object)
            );
        var act = await ctrl.GetAppConfig("001", new EnvString() { Value = "DEV" });

        Assert.IsNotNull(act);
        Assert.IsNotNull(act.Value);
        Assert.IsInstanceOfType(act.Value, typeof(List<ApiConfigVM>));
        Assert.AreEqual(2, act.Value.Count);

        App newApp1()
        {
            return new App()
            {
                Enabled = false
            };
        }
        appService = new Mock<IAppService>();
        appService.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(newApp1);

        ctrl = new ConfigController(
            configService.Object,
            appService.Object,
            memoryCache, 
            meterService.Object,
            new AgileConfig.Server.Apisite.Controllers.ConfigController(configService.Object, appService.Object, userSErvice.Object, eventBus.Object)
            );
        act = await ctrl.GetAppConfig("001", new EnvString() { Value = "DEV" });

        Assert.IsNotNull(act);
        Assert.IsNull(act.Value);
        Assert.IsInstanceOfType(act.Result, typeof(NotFoundResult));
    }
}
