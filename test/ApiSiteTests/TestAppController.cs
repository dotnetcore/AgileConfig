using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Application;
using AgileConfig.Server.Common;
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
        var applicationManagementService = new Mock<IApplicationManagementService>();
        var ctl = new AppController(appService.Object, userService.Object, configService.Object, settingService.Object,
            applicationManagementService.Object);
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
        var userService = new Mock<IUserService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        var applicationManagementService = new Mock<IApplicationManagementService>();

        var ctl = new AppController(appService.Object, userService.Object, configService.Object, settingService.Object,
            applicationManagementService.Object);

        ctl.ControllerContext.HttpContext = new DefaultHttpContext();

        Assert.ThrowsExactly<ArgumentNullException>(() => { ctl.Add(null).GetAwaiter().GetResult(); });

        applicationManagementService
            .Setup(x => x.CreateAsync(It.Is<CreateApplicationCommand>(command => command.Id == "01")))
            .ReturnsAsync(ApplicationResult<App>.Failure(ApplicationError.Conflict));
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
        applicationManagementService
            .Setup(x => x.CreateAsync(It.Is<CreateApplicationCommand>(command => command.Id == "02")))
            .ReturnsAsync(ApplicationResult<App>.Failure(ApplicationError.OperationFailed));
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

        applicationManagementService
            .Setup(x => x.CreateAsync(It.Is<CreateApplicationCommand>(command => command.Id == "02")))
            .ReturnsAsync(ApplicationResult<App>.Success(new App { Id = "02" }));
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

    [TestMethod]
    public async Task SearchAndReadEndpoints_ReturnAppsForAdministrators()
    {
        var appService = new Mock<IAppService>();
        var userService = new Mock<IUserService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        var controller = BuildController(appService, configService, settingService, userService);
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim("id", "admin-user")], "test"));

        var parent = new App { Id = "parent", Name = "Parent", Enabled = true };
        var child = new App { Id = "child", Name = "Child", Enabled = true };
        appService.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), "admin-user", true))
            .ReturnsAsync((new List<App> { parent }, 1L));
        appService.Setup(x => x.SearchGroupedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), "admin-user", true))
            .ReturnsAsync((new List<GroupedApp> { new() { App = parent, Children = [new GroupedApp { App = child }] } }, 1L));
        appService.Setup(x => x.GetInheritancedAppsAsync(It.IsAny<string>())).ReturnsAsync(new List<App>());
        appService.Setup(x => x.GetAsync("parent")).ReturnsAsync(parent);
        userService.Setup(x => x.GetUserRolesAsync("admin-user"))
            .ReturnsAsync(new List<Role> { new() { Id = SystemRoleConstants.AdminId } });

        Assert.IsInstanceOfType(await controller.Search(null, null, null, "name", "ascend", false), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Search(null, null, null, "name", "ascend", true), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Get("parent"), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Get("missing"), typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task AppManagementAndExportEndpoints_ExecuteExpectedServiceOperations()
    {
        var appService = new Mock<IAppService>();
        var userService = new Mock<IUserService>();
        var configService = new Mock<IConfigService>();
        var settingService = new Mock<ISettingService>();
        var applicationManagement = new Mock<IApplicationManagementService>();
        var controller = new AppController(appService.Object, userService.Object, configService.Object, settingService.Object,
            applicationManagement.Object);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim("id", "admin-user")], "test"));

        var app = new App { Id = "app-1", Name = "App", Enabled = true, Type = AppType.PRIVATE };
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(app);
        appService.Setup(x => x.GetInheritancedAppsAsync("app-1")).ReturnsAsync(new List<App>());
        appService.Setup(x => x.UpdateAsync(app)).ReturnsAsync(true);
        appService.Setup(x => x.SearchAsync("app-1", null, null, nameof(App.Id), "ascend", 1, 1, "admin-user", false))
            .ReturnsAsync((new List<App> { app }, 1L));
        appService.Setup(x => x.SaveUserAppAuth("app-1", It.IsAny<List<string>>())).ReturnsAsync(true);
        appService.Setup(x => x.GetUserAppAuth("app-1")).ReturnsAsync(new List<User> { new() { Id = "user-1" } });
        appService.Setup(x => x.GetAllInheritancedAppsAsync()).ReturnsAsync(new List<App> { app, new() { Id = "hidden", Enabled = false } });
        appService.Setup(x => x.GetAppGroups()).ReturnsAsync(new List<string> { "z", "a" });
        userService.Setup(x => x.GetUserRolesAsync("admin-user"))
            .ReturnsAsync(new List<Role> { new() { Id = SystemRoleConstants.AdminId } });
        settingService.Setup(x => x.GetEnvironmentList()).ReturnsAsync(["DEV", "DEV", ""]);
        configService.Setup(x => x.GetByAppIdAsync("app-1", "DEV")).ReturnsAsync(new List<Config>
        {
            new() { Key = "key", Value = "value", Group = "group" }
        });
        applicationManagement.Setup(x => x.UpdateAsync(It.IsAny<UpdateApplicationCommand>()))
            .ReturnsAsync(ApplicationResult<App>.Failure(ApplicationError.ValidationFailed));
        applicationManagement.Setup(x => x.DeleteAsync(It.IsAny<DeleteApplicationCommand>()))
            .ReturnsAsync(ApplicationResult<App>.Success(app));

        Assert.IsInstanceOfType(await controller.DisableOrEnable("app-1"), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Edit(new AppVM { Id = "app-1" }), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Delete("app-1"), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.Export(new AppExportRequest { AppIds = ["app-1", "APP-1"] }), typeof(FileContentResult));
        Assert.IsInstanceOfType(await controller.InheritancedApps("app-1"), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.SaveAppAuth(new AppAuthVM { AppId = "app-1", AuthorizedUsers = ["user-1"] }), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.GetUserAppAuth("app-1"), typeof(JsonResult));
        Assert.IsInstanceOfType(await controller.GetAppGroups(), typeof(JsonResult));
    }
}
