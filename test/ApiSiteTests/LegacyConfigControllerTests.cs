using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Apisite.Controllers;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public class LegacyConfigControllerTests
{
    [TestMethod]
    public async Task Mutations_MapApplicationOutcomesToLegacyResponses()
    {
        var dependencies = new Dependencies();
        var controller = dependencies.CreateController();
        var env = new EnvString { Value = "DEV" };
        var model = new ConfigVM { Id = "config-1", AppId = "app-1", Key = "key", Value = "value" };

        dependencies.ConfigurationManagement
            .Setup(x => x.CreateAsync(It.IsAny<CreateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Failure(ApplicationError.Conflict));
        Assert.IsNotNull(await controller.Add(model, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.CreateAsync(It.IsAny<CreateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Success(new Config { Id = model.Id }));
        Assert.IsNotNull(await controller.Add(model, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Failure(ApplicationError.NotFound));
        dependencies.ConfigService.Setup(x => x.GetAsync(model.Id, env.Value)).ReturnsAsync((Config)null);
        Assert.IsNotNull(await controller.Edit(model, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Failure(ApplicationError.Conflict));
        Assert.IsNotNull(await controller.Edit(model, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Success(new Config { Id = model.Id }));
        Assert.IsNotNull(await controller.Edit(model, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.DeleteAsync(It.IsAny<DeleteConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Failure(ApplicationError.NotFound));
        Assert.IsNotNull(await controller.Delete(model.Id, env) as JsonResult);

        dependencies.ConfigurationManagement
            .Setup(x => x.DeleteAsync(It.IsAny<DeleteConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Success(new Config { Id = model.Id }));
        Assert.IsNotNull(await controller.Delete(model.Id, env) as JsonResult);

        dependencies.ReleaseManagement
            .Setup(x => x.PublishAsync(It.IsAny<PublishConfigurationsCommand>()))
            .ReturnsAsync(ApplicationResult<PublishTimeline>.Success(new PublishTimeline()));
        dependencies.ReleaseManagement
            .Setup(x => x.RollbackAsync(It.IsAny<RollbackConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<PublishTimeline>.Failure(ApplicationError.OperationFailed));
        Assert.IsNotNull(await controller.Publish(new PublishLogVM { AppId = "app-1", Ids = [], Log = "publish" }, env) as JsonResult);
        Assert.IsNotNull(await controller.Rollback("timeline-1", env) as JsonResult);
    }

    [TestMethod]
    public async Task SearchAndReadEndpoints_FilterSortAndReturnLegacyPayloads()
    {
        var dependencies = new Dependencies();
        var controller = dependencies.CreateController();
        var env = new EnvString { Value = "DEV" };
        var configs = new List<Config>
        {
            new() { Id = "disabled", Key = "disabled", Status = ConfigStatus.Deleted, CreateTime = DateTime.UtcNow },
            new() { Id = "old", Key = "old", Group = "z", Status = ConfigStatus.Enabled, OnlineStatus = OnlineStatus.WaitPublish, CreateTime = DateTime.UtcNow.AddMinutes(-1), EditStatus = EditStatus.Add },
            new() { Id = "new", Key = "new", Group = "a", Status = ConfigStatus.Enabled, OnlineStatus = OnlineStatus.Online, CreateTime = DateTime.UtcNow, EditStatus = EditStatus.Edit }
        };
        dependencies.ConfigService.Setup(x => x.Search("app-1", It.IsAny<string>(), It.IsAny<string>(), env.Value))
            .ReturnsAsync(configs);
        dependencies.ConfigService.Setup(x => x.GetAllConfigsAsync(env.Value)).ReturnsAsync(configs);
        dependencies.ConfigService.Setup(x => x.GetAsync("new", env.Value)).ReturnsAsync(configs.Last());
        dependencies.ConfigService.Setup(x => x.GetAsync("missing", env.Value)).ReturnsAsync((Config)null);

        Assert.IsNotNull(await controller.All(env.Value) as JsonResult);
        Assert.IsNotNull(await controller.Search("app-1", null, null, null, "group", "ascend", env, 10, 1) as JsonResult);
        Assert.IsNotNull(await controller.Search("app-1", null, null, OnlineStatus.Online, "createTime", "descend", env, 10, 1) as JsonResult);
        Assert.IsNotNull(await controller.Get("new", env) as JsonResult);
        Assert.IsNotNull(await controller.Get("missing", env) as JsonResult);
        Assert.IsNotNull(await controller.WaitPublishStatus("app-1", env) as JsonResult);
    }

    [TestMethod]
    public async Task BatchAndContentEndpoints_UseConfigurationServiceResults()
    {
        var dependencies = new Dependencies();
        var controller = dependencies.CreateController();
        var env = new EnvString { Value = "DEV" };
        var existing = new List<Config>
        {
            new() { Id = "old", AppId = "app-1", Key = "old", Value = "old", EditStatus = EditStatus.Commit }
        };
        dependencies.ConfigService.Setup(x => x.GetByAppIdAsync("app-1", env.Value)).ReturnsAsync(existing);
        dependencies.ConfigService.Setup(x => x.GenerateKey(It.IsAny<Config>())).Returns<Config>(config =>
            string.IsNullOrEmpty(config.Group) ? config.Key : $"{config.Group}:{config.Key}");
        dependencies.ConfigService.Setup(x => x.AddRangeAsync(It.IsAny<List<Config>>(), env.Value)).ReturnsAsync(true);
        dependencies.ConfigService.Setup(x => x.UpdateAsync(It.IsAny<List<Config>>(), env.Value)).ReturnsAsync(true);
        dependencies.ConfigService.Setup(x => x.IsPublishedAsync(It.IsAny<string>(), env.Value)).ReturnsAsync(false);
        dependencies.ConfigService.Setup(x => x.GetAsync("old", env.Value)).ReturnsAsync(existing.Single());
        dependencies.ConfigService.Setup(x => x.CancelEdit(It.IsAny<List<string>>(), env.Value)).ReturnsAsync(true);
        dependencies.ConfigService.Setup(x => x.SaveJsonAsync(It.IsAny<string>(), "app-1", env.Value, It.IsAny<bool>())).ReturnsAsync(true);
        dependencies.ConfigService.Setup(x => x.ValidateKvString("key=value")).Returns((true, ""));
        dependencies.ConfigService.Setup(x => x.SaveKvListAsync("key=value", "app-1", env.Value, It.IsAny<bool>())).ReturnsAsync(true);

        var add = await controller.AddRange(new List<ConfigVM>
        {
            new() { AppId = "app-1", Key = "new", Value = "value" }
        }, env) as JsonResult;
        Assert.IsNotNull(add);

        var duplicate = await controller.AddRange(new List<ConfigVM>
        {
            new() { AppId = "app-1", Key = "old", Value = "value" }
        }, env) as JsonResult;
        Assert.IsNotNull(duplicate);

        Assert.IsNotNull(await controller.DeleteSome(new List<string> { "old" }, env) as JsonResult);
        Assert.IsNotNull(await controller.CancelEdit("old", env) as JsonResult);
        Assert.IsNotNull(await controller.CancelSomeEdit(new List<string> { "old" }, env) as JsonResult);
        Assert.IsNotNull(await controller.GetKvList("app-1", env) as JsonResult);
        Assert.IsNotNull(await controller.GetJson("app-1", env) as JsonResult);
        Assert.IsNotNull(await controller.SaveJson(new SaveJsonVM { json = "{}", isPatch = true }, "app-1", env) as JsonResult);
        Assert.IsNotNull(await controller.SaveKvList(new SaveKVListVM { str = "key=value", isPatch = false }, "app-1", env) as JsonResult);
    }

    [TestMethod]
    public async Task SynchronizationAndHistoryEndpoints_HandleFoundAndMissingApps()
    {
        var dependencies = new Dependencies();
        var controller = dependencies.CreateController();
        var env = new EnvString { Value = "DEV" };
        dependencies.AppService.Setup(x => x.GetAsync("missing")).ReturnsAsync((App)null);
        dependencies.AppService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });
        dependencies.ConfigService.Setup(x => x.EnvSync("app-1", "DEV", It.IsAny<List<string>>())).ReturnsAsync(true);
        dependencies.ConfigService.Setup(x => x.GetPublishDetailListAsync("app-1", env.Value)).ReturnsAsync(new List<PublishDetail>
        {
            new() { Version = 2, PublishTimelineId = "t2" },
            new() { Version = 1, PublishTimelineId = "t1" }
        });
        dependencies.ConfigService.Setup(x => x.GetPublishTimeLineNodeAsync(It.IsAny<string>(), env.Value))
            .ReturnsAsync(new PublishTimeline());
        dependencies.ConfigService.Setup(x => x.GetConfigPublishedHistory("config-1", env.Value))
            .ReturnsAsync(new List<PublishDetail> { new() { Version = 1, PublishTimelineId = "t1" } });

        Assert.IsNotNull(await controller.SyncEnv(new List<string> { "TEST" }, "missing", "DEV") as JsonResult);
        Assert.IsNotNull(await controller.SyncEnv(new List<string> { "TEST" }, "app-1", "DEV") as JsonResult);
        Assert.IsNotNull(await controller.PublishHistory("app-1", env) as JsonResult);
        Assert.IsNotNull(await controller.ConfigPublishedHistory("config-1", env) as JsonResult);
    }

    private sealed class Dependencies
    {
        public Mock<IConfigService> ConfigService { get; } = new();
        public Mock<IAppService> AppService { get; } = new();
        public Mock<ITinyEventBus> EventBus { get; } = new();
        public Mock<IConfigurationManagementService> ConfigurationManagement { get; } = new();
        public Mock<IReleaseManagementService> ReleaseManagement { get; } = new();

        public ConfigController CreateController()
        {
            var controller = new ConfigController(
                ConfigService.Object,
                AppService.Object,
                EventBus.Object,
                ConfigurationManagement.Object,
                ReleaseManagement.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            return controller;
        }
    }
}
