using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace ApiSiteTests;

[TestClass]
public class TestAppController
{
    private static AppController BuildController(Mock<IAppService> appService, Mock<IConfigService> configService,
        Mock<ISettingService> settingService, Mock<IUserService> userService)
    {
        var eventBus = new Mock<ITinyEventBus>();
        var ctl = new AppController(appService.Object, userService.Object, configService.Object, settingService.Object,
            eventBus.Object);
        ctl.ControllerContext.HttpContext = new DefaultHttpContext();
        return ctl;
    }

    private static IFormFile BuildImportFile(AppExportFileVM file)
    {
        var json = JsonConvert.SerializeObject(file);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "apps.json");
    }

    [TestMethod]
    public async Task TestAdd()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("en-US", false);

        var appService = new Mock<IAppService>();
        var logService = new Mock<ISysLogService>();
        var userService = new Mock<IUserService>();
        var permissionService = new Mock<IPermissionService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        var eventBus = new Mock<ITinyEventBus>();

        var ctl = new AppController(appService.Object, userService.Object, configService.Object, settingService.Object,
            eventBus.Object);

        ctl.ControllerContext.HttpContext = new DefaultHttpContext();

        Assert.ThrowsExactly<ArgumentNullException>(() => { ctl.Add(null).GetAwaiter().GetResult(); });

        appService.Setup(s => s.GetAsync("01")).ReturnsAsync(new App());
        var result = await ctl.Add(new AppVM
        {
            Id = "01"
        });
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        var jr = result as JsonResult;
        Assert.IsNotNull(jr.Value);
        Console.WriteLine(jr.Value.ToString());
        //Assert.IsTrue(jr.Value.ToString().Contains("Ӧ��Id�Ѵ��ڣ�����������"));
        App nullApp = null;

        appService.Setup(s => s.GetAsync("02")).ReturnsAsync(nullApp);
        appService.Setup(s => s.AddAsync(It.IsAny<App>())).ReturnsAsync(false);
        result = await ctl.Add(new AppVM
        {
            Id = "02"
        });
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        jr = result as JsonResult;
        Assert.IsNotNull(jr.Value);
        Console.WriteLine(jr.Value.ToString());
        Assert.IsTrue(jr.Value.ToString().Contains("success = False"));

        appService.Setup(s => s.AddAsync(It.IsAny<App>())).ReturnsAsync(true);
        appService.Setup(s => s.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>())).ReturnsAsync(true);
        Console.WriteLine(CultureInfo.CurrentUICulture);
        result = await ctl.Add(new AppVM
        {
            Id = "02"
        });
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(JsonResult));
        jr = result as JsonResult;
        Assert.IsNotNull(jr.Value);
        Console.WriteLine(jr.Value.ToString());
        Assert.IsTrue(jr.Value.ToString().Contains("success = True"));
    }

    [TestMethod]
    public async Task PreviewImport_ShouldRejectDuplicatesMissingParentsAndCycles()
    {
        var appService = new Mock<IAppService>();
        var userService = new Mock<IUserService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        appService.Setup(x => x.GetAllAppsAsync()).ReturnsAsync(new List<App>
        {
            new() { Id = "existing-id", Name = "Existing Name" }
        });

        var controller = BuildController(appService, configService, settingService, userService);
        var file = new AppExportFileVM
        {
            Apps = new List<AppExportItemVM>
            {
                new()
                {
                    App = new AppExportAppVM { Id = "existing-id", Name = "new-name", InheritancedApps = new List<string> { "missing-parent" } },
                    Envs = new Dictionary<string, List<AppExportConfigVM>>()
                },
                new()
                {
                    App = new AppExportAppVM { Id = "cycle-a", Name = "Existing Name", InheritancedApps = new List<string> { "cycle-b" } },
                    Envs = new Dictionary<string, List<AppExportConfigVM>>()
                },
                new()
                {
                    App = new AppExportAppVM { Id = "cycle-b", Name = "cycle-b", InheritancedApps = new List<string> { "cycle-a" } },
                    Envs = new Dictionary<string, List<AppExportConfigVM>>()
                }
            }
        };

        var result = await controller.PreviewImport(BuildImportFile(file));
        var json = result as JsonResult;
        var payload = JsonConvert.SerializeObject(json?.Value);

        Assert.IsNotNull(json);
        Assert.IsTrue(payload.Contains("\"success\":false"));
        Assert.IsTrue(payload.Contains("AppId already exists: existing-id"));
        Assert.IsTrue(payload.Contains("Existing Name"));
        Assert.IsTrue(payload.Contains("missing parent 'missing-parent'"));
        Assert.IsTrue(payload.Contains("Cyclic inheritance detected"));
    }

    [TestMethod]
    public async Task Import_ShouldCreateAppsInTopologicalOrderAndAddConfigsAsNew()
    {
        var appService = new Mock<IAppService>();
        var userService = new Mock<IUserService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        appService.Setup(x => x.GetAllAppsAsync()).ReturnsAsync(new List<App>());
        userService.Setup(x => x.GetUserRolesAsync(It.IsAny<string>())).ReturnsAsync(new List<Role>());

        var addedApps = new List<App>();
        var addedInheritance = new List<List<AppInheritanced>>();
        var addedConfigs = new List<Config>();
        appService.Setup(x => x.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>()))
            .Callback<App, List<AppInheritanced>>((app, links) =>
            {
                addedApps.Add(app);
                addedInheritance.Add(links);
            })
            .ReturnsAsync(true);
        configService.Setup(x => x.AddAsync(It.IsAny<Config>(), It.IsAny<string>()))
            .Callback<Config, string>((config, _) => addedConfigs.Add(config))
            .ReturnsAsync(true);

        var controller = BuildController(appService, configService, settingService, userService);
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("id", "tester")
            }, "mock"));

        var importFile = new AppExportFileVM
        {
            Apps = new List<AppExportItemVM>
            {
                new()
                {
                    App = new AppExportAppVM { Id = "child", Name = "Child", InheritancedApps = new List<string> { "parent" } },
                    Envs = new Dictionary<string, List<AppExportConfigVM>>
                    {
                        ["DEV"] = new() { new AppExportConfigVM { Key = "child-key", Value = "child-value" } }
                    }
                },
                new()
                {
                    App = new AppExportAppVM { Id = "parent", Name = "Parent", Inheritanced = true, InheritancedApps = new List<string>() },
                    Envs = new Dictionary<string, List<AppExportConfigVM>>
                    {
                        ["DEV"] = new() { new AppExportConfigVM { Key = "parent-key", Value = "parent-value" } }
                    }
                }
            }
        };

        var result = await controller.Import(new AppImportRequest { File = importFile });
        var json = result as JsonResult;
        var payload = JsonConvert.SerializeObject(json?.Value);

        Assert.IsNotNull(json);
        Assert.IsTrue(payload.Contains("\"success\":true"));
        CollectionAssert.AreEqual(new[] { "parent", "child" }, addedApps.Select(x => x.Id).ToArray());
        Assert.AreEqual("parent", addedInheritance.Last().Single().InheritancedAppId);
        Assert.AreEqual(2, addedConfigs.Count);
        Assert.IsTrue(addedConfigs.All(x => x.EditStatus == EditStatus.Add));
        Assert.IsTrue(addedConfigs.All(x => x.OnlineStatus == OnlineStatus.WaitPublish));
        Assert.IsTrue(addedConfigs.All(x => x.Status == ConfigStatus.Enabled));
    }
}
