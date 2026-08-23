using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Releases;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class ReleaseManagementServiceTests
{
    [TestMethod]
    public async Task PublishAsync_PublishesWithCurrentUserAndFiresEvent()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.Publish(
                "app-1", new[] { "config-1" }, "release log", "user-1", "TEST"))
            .ReturnsAsync((true, "release-1"));
        var timeline = new PublishTimeline
        {
            Id = "release-1", AppId = "app-1", Env = "TEST", Version = 4
        };
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("release-1", "TEST"))
            .ReturnsAsync(timeline);
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("operator");
        currentUser.Setup(x => x.GetUserIdAsync()).ReturnsAsync("user-1");
        var eventBus = new Mock<ITinyEventBus>();
        var service = CreateService(appService, configService, currentUser, eventBus);

        var result = await service.PublishAsync(new PublishConfigurationsCommand(
            "app-1", new[] { "config-1" }, "release log", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(timeline, result.Value);
        configService.VerifyAll();
        eventBus.Verify(x => x.Fire(It.Is<PublishConfigSuccessful>(e =>
            e.PublishTimeline == timeline && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task PublishAsync_ReturnsNotFoundWhenApplicationDoesNotExist()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("missing")).ReturnsAsync((App)null);
        var configService = new Mock<IConfigService>();
        var currentUser = new Mock<ICurrentUserAccessor>();
        var service = CreateService(appService, configService, currentUser);

        var result = await service.PublishAsync(new PublishConfigurationsCommand(
            "missing", null, null, "TEST"));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        configService.Verify(x => x.Publish(
            It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        currentUser.Verify(x => x.GetUserIdAsync(), Times.Never);
    }

    [TestMethod]
    public async Task PublishAsync_WhenTimelineCannotBeLoaded_ReturnsFallbackAndStillFiresEvent()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.Publish("app-1", null, "log", null, "TEST"))
            .ReturnsAsync((true, "missing-release"));
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("missing-release", "TEST"))
            .ReturnsAsync((PublishTimeline)null);
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.Setup(x => x.GetUserIdAsync()).ReturnsAsync((string)null);
        var eventBus = new Mock<ITinyEventBus>();
        var service = CreateService(appService, configService, currentUser, eventBus);

        var result = await service.PublishAsync(new PublishConfigurationsCommand(
            "app-1", null, "log", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("missing-release", result.Value.Id);
        Assert.AreEqual("app-1", result.Value.AppId);
        Assert.AreEqual("TEST", result.Value.Env);
        eventBus.Verify(x => x.Fire(It.Is<PublishConfigSuccessful>(e =>
            e.PublishTimeline == result.Value)), Times.Once);
    }

    [TestMethod]
    public async Task RollbackAsync_RollsBackAndFiresEvent()
    {
        var timeline = new PublishTimeline
        {
            Id = "release-1", AppId = "app-1", Env = "TEST", Version = 2
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("release-1", "TEST"))
            .ReturnsAsync(timeline);
        configService.Setup(x => x.RollbackAsync("release-1", "TEST")).ReturnsAsync(true);
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("operator");
        var eventBus = new Mock<ITinyEventBus>();
        var service = CreateService(appService, configService, currentUser, eventBus);

        var result = await service.RollbackAsync(new RollbackConfigurationCommand("release-1", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(timeline, result.Value);
        configService.Verify(x => x.RollbackAsync("release-1", "TEST"), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<RollbackConfigSuccessful>(e =>
            e.TimelineNode == timeline && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task RollbackAsync_ReturnsNotFoundWithoutCallingRollback()
    {
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("missing", "TEST"))
            .ReturnsAsync((PublishTimeline)null);
        var service = CreateService(appService, configService);

        var result = await service.RollbackAsync(new RollbackConfigurationCommand("missing", "TEST"));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        configService.Verify(x => x.RollbackAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task GetAsync_OnlyReturnsReleaseOwnedByApplication()
    {
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("release-1", "TEST"))
            .ReturnsAsync(new PublishTimeline { Id = "release-1", AppId = "other-app", Env = "TEST" });
        var service = CreateService(appService, configService);

        var result = await service.GetAsync("app-1", "TEST", "release-1");

        Assert.IsNull(result);
    }

    private static ReleaseManagementService CreateService(
        Mock<IAppService> appService,
        Mock<IConfigService> configService,
        Mock<ICurrentUserAccessor> currentUser = null,
        Mock<ITinyEventBus> eventBus = null)
    {
        return new ReleaseManagementService(
            appService.Object,
            configService.Object,
            (currentUser ?? new Mock<ICurrentUserAccessor>()).Object,
            (eventBus ?? new Mock<ITinyEventBus>()).Object,
            TimeProvider.System);
    }
}
