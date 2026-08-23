using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class ConfigurationManagementServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 8, 24, 10, 30, 0, TimeSpan.FromHours(8));

    [TestMethod]
    public async Task CreateAsync_CreatesPendingConfigurationAndPublishesEvent()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetByAppIdKeyEnv("app-1", "db", "connection", "TEST"))
            .ReturnsAsync((Config)null);
        Config saved = null;
        configService.Setup(x => x.AddAsync(It.IsAny<Config>(), "TEST"))
            .Callback<Config, string>((config, _) => saved = config)
            .ReturnsAsync(true);
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("operator");
        var eventBus = new Mock<ITinyEventBus>();
        var service = CreateService(appService, configService, currentUser, eventBus);

        var result = await service.CreateAsync(new CreateConfigurationCommand(
            "", "app-1", "db", "connection", "value", "description", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(saved, result.Value);
        Assert.IsFalse(string.IsNullOrEmpty(saved.Id));
        Assert.AreEqual(ConfigStatus.Enabled, saved.Status);
        Assert.AreEqual(EditStatus.Add, saved.EditStatus);
        Assert.AreEqual(OnlineStatus.WaitPublish, saved.OnlineStatus);
        Assert.AreEqual(FixedNow.ToLocalTime().DateTime, saved.CreateTime);
        eventBus.Verify(x => x.Fire(It.Is<AddConfigSuccessful>(e =>
            e.Config == saved && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsConflictWithoutPersistingDuplicate()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetByAppIdKeyEnv("app-1", "db", "connection", "TEST"))
            .ReturnsAsync(new Config { Id = "existing" });
        var service = CreateService(appService, configService);

        var result = await service.CreateAsync(new CreateConfigurationCommand(
            null, "app-1", "db", "connection", "value", null, "TEST"));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        configService.Verify(x => x.AddAsync(It.IsAny<Config>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_MarksPublishedConfigurationAsEdited()
    {
        var existing = new Config
        {
            Id = "config-1",
            AppId = "app-1",
            Group = "old",
            Key = "key",
            Value = "old-value",
            Status = ConfigStatus.Enabled,
            EditStatus = EditStatus.Commit,
            OnlineStatus = OnlineStatus.Online,
            Env = "TEST"
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST")).ReturnsAsync(existing);
        configService.Setup(x => x.GetByAppIdAsync("app-1", "TEST"))
            .ReturnsAsync(new List<Config> { existing });
        configService.Setup(x => x.GetByAppIdKeyEnv("app-1", "new", "key", "TEST"))
            .ReturnsAsync((Config)null);
        configService.Setup(x => x.IsPublishedAsync("config-1", "TEST")).ReturnsAsync(true);
        configService.Setup(x => x.UpdateAsync(existing, "TEST")).ReturnsAsync(true);
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("operator");
        var eventBus = new Mock<ITinyEventBus>();
        var service = CreateService(appService, configService, currentUser, eventBus);

        var result = await service.UpdateAsync(new UpdateConfigurationCommand(
            "config-1", "app-1", "new", "key", "new-value", "description", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(EditStatus.Edit, existing.EditStatus);
        Assert.AreEqual(OnlineStatus.WaitPublish, existing.OnlineStatus);
        Assert.AreEqual(FixedNow.DateTime, existing.UpdateTime);
        eventBus.Verify(x => x.Fire(It.Is<EditConfigSuccessful>(e => e.Config == existing)), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_DescriptionOnlyChangeKeepsPublicationState()
    {
        var existing = new Config
        {
            Id = "config-1",
            AppId = "app-1",
            Group = "group",
            Key = "key",
            Value = "value",
            Status = ConfigStatus.Enabled,
            EditStatus = EditStatus.Commit,
            OnlineStatus = OnlineStatus.Online,
            Env = "TEST"
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST")).ReturnsAsync(existing);
        configService.Setup(x => x.GetByAppIdAsync("app-1", "TEST"))
            .ReturnsAsync(new List<Config> { existing });
        configService.Setup(x => x.UpdateAsync(existing, "TEST")).ReturnsAsync(true);
        var service = CreateService(appService, configService);

        var result = await service.UpdateAsync(new UpdateConfigurationCommand(
            "config-1", "app-1", "group", "key", "value", "new description", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(EditStatus.Commit, existing.EditStatus);
        Assert.AreEqual(OnlineStatus.Online, existing.OnlineStatus);
        configService.Verify(x => x.IsPublishedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsConflictForChangedDuplicateKey()
    {
        var existing = new Config
        {
            Id = "config-1", AppId = "app-1", Group = "old", Key = "key", Value = "value"
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST")).ReturnsAsync(existing);
        configService.Setup(x => x.GetByAppIdAsync("app-1", "TEST"))
            .ReturnsAsync(new List<Config> { existing });
        configService.Setup(x => x.GetByAppIdKeyEnv("app-1", "new", "key", "TEST"))
            .ReturnsAsync(new Config { Id = "other" });
        var service = CreateService(appService, configService);

        var result = await service.UpdateAsync(new UpdateConfigurationCommand(
            "config-1", "app-1", "new", "key", "value", null, "TEST"));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        configService.Verify(x => x.UpdateAsync(It.IsAny<Config>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_DeletesUnpublishedConfigurationImmediately()
    {
        var existing = new Config
        {
            Id = "config-1", AppId = "app-1", Status = ConfigStatus.Enabled,
            EditStatus = EditStatus.Add, OnlineStatus = OnlineStatus.WaitPublish
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST")).ReturnsAsync(existing);
        configService.Setup(x => x.IsPublishedAsync("config-1", "TEST")).ReturnsAsync(false);
        configService.Setup(x => x.UpdateAsync(existing, "TEST")).ReturnsAsync(true);
        var service = CreateService(appService, configService);

        var result = await service.DeleteAsync(new DeleteConfigurationCommand("config-1", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(ConfigStatus.Deleted, existing.Status);
        Assert.AreEqual(EditStatus.Deleted, existing.EditStatus);
        Assert.AreEqual(OnlineStatus.WaitPublish, existing.OnlineStatus);
        configService.Verify(x => x.UpdateAsync(existing, "TEST"), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_KeepsPublishedConfigurationUntilRelease()
    {
        var existing = new Config
        {
            Id = "config-1", AppId = "app-1", Status = ConfigStatus.Enabled,
            EditStatus = EditStatus.Commit, OnlineStatus = OnlineStatus.Online
        };
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST")).ReturnsAsync(existing);
        configService.Setup(x => x.IsPublishedAsync("config-1", "TEST")).ReturnsAsync(true);
        configService.Setup(x => x.UpdateAsync(existing, "TEST")).ReturnsAsync(true);
        var service = CreateService(appService, configService);

        var result = await service.DeleteAsync(new DeleteConfigurationCommand("config-1", "TEST"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(ConfigStatus.Enabled, existing.Status);
        Assert.AreEqual(EditStatus.Deleted, existing.EditStatus);
        Assert.AreEqual(OnlineStatus.WaitPublish, existing.OnlineStatus);
    }

    [TestMethod]
    public async Task GetAsync_HidesConfigurationFromAnotherApplicationOrDeletedState()
    {
        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("config-1", "TEST"))
            .ReturnsAsync(new Config
            {
                Id = "config-1", AppId = "app-1", Status = ConfigStatus.Enabled,
                EditStatus = EditStatus.Deleted
            });
        var service = CreateService(new Mock<IAppService>(), configService);

        var result = await service.GetAsync("app-1", "TEST", "config-1");

        Assert.IsNull(result);
    }

    private static ConfigurationManagementService CreateService(
        Mock<IAppService> appService,
        Mock<IConfigService> configService,
        Mock<ICurrentUserAccessor> currentUser = null,
        Mock<ITinyEventBus> eventBus = null)
    {
        return new ConfigurationManagementService(
            appService.Object,
            configService.Object,
            (currentUser ?? new Mock<ICurrentUserAccessor>()).Object,
            (eventBus ?? new Mock<ITinyEventBus>()).Object,
            new FixedTimeProvider(FixedNow));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            _now = now;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _now.ToUniversalTime();
        }

    }
}
