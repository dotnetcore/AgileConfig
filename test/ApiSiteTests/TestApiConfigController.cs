using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiSiteTests;

[TestClass]
public class TestApiConfigController
{
    [TestMethod]
    public async Task GetAppConfig_WithValidApp_ReturnsConfigs()
    {
        App newApp()
        {
            return new App
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
        configService.Setup(s => s.GetPublishedConfigsByAppIdWithInheritance(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(newConfigs);

        IMemoryCache memoryCache = null;
        var userSErvice = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var meterService = new Mock<IMeterService>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Basic MDAxOjE=";

        var ctrl = new AgileConfig.Server.Apisite.Controllers.api.ConfigController(
            configService.Object,
            appService.Object,
            memoryCache,
            meterService.Object,
            new ConfigController(configService.Object, appService.Object, userSErvice.Object, eventBus.Object)
        );
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var act = await ctrl.GetAppConfig("001", new EnvString { Value = "DEV" });

        Assert.IsNotNull(act);
        Assert.IsNotNull(act.Value);
        Assert.IsInstanceOfType(act.Value, typeof(List<ApiConfigVM>));
        Assert.AreEqual(2, act.Value.Count);
    }

    [TestMethod]
    public async Task GetAppConfig_WithDisabledApp_ReturnsNotFound()
    {
        App newApp1()
        {
            return new App
            {
                Enabled = false
            };
        }

        var appService = new Mock<IAppService>();
        appService.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync(newApp1);
        var configService = new Mock<IConfigService>();
        IMemoryCache memoryCache = null;
        var userSErvice = new Mock<IUserService>();
        var eventBus = new Mock<ITinyEventBus>();
        var meterService = new Mock<IMeterService>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "Basic MDAxOjE=";

        var ctrl = new AgileConfig.Server.Apisite.Controllers.api.ConfigController(
            configService.Object,
            appService.Object,
            memoryCache,
            meterService.Object,
            new ConfigController(configService.Object, appService.Object, userSErvice.Object, eventBus.Object)
        );
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var act = await ctrl.GetAppConfig("001", new EnvString { Value = "DEV" });

        Assert.IsNotNull(act);
        Assert.IsNull(act.Value);
        Assert.IsInstanceOfType(act.Result, typeof(NotFoundResult));
    }
}