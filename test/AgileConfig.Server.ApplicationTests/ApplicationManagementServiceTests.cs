using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class ApplicationManagementServiceTests
{
    [TestMethod]
    public async Task GetAllAsync_ReturnsApplicationsWithTheirInheritance()
    {
        var appService = new Mock<IAppService>();
        var first = new App { Id = "app-1" };
        var second = new App { Id = "app-2" };
        appService.Setup(x => x.GetAllAppsAsync()).ReturnsAsync(new List<App> { first, second });
        appService.Setup(x => x.GetInheritancedAppsAsync("app-1"))
            .ReturnsAsync(new List<App> { new() { Id = "base-1" } });
        appService.Setup(x => x.GetInheritancedAppsAsync("app-2"))
            .ReturnsAsync(new List<App>());

        var result = await CreateService(appService, new Mock<ITinyEventBus>()).GetAllAsync();

        Assert.AreEqual(2, result.Count);
        Assert.AreSame(first, result[0].Application);
        CollectionAssert.AreEqual(new[] { "base-1" }, result[0].InheritedApplicationIds.ToArray());
        Assert.AreSame(second, result[1].Application);
        Assert.AreEqual(0, result[1].InheritedApplicationIds.Count);
    }

    [TestMethod]
    public async Task GetAsync_WhenApplicationDoesNotExist_DoesNotLoadInheritance()
    {
        var appService = new Mock<IAppService>();
        appService.Setup(x => x.GetAsync("missing")).ReturnsAsync((App)null);

        var result = await CreateService(appService, new Mock<ITinyEventBus>()).GetAsync("missing");

        Assert.IsNull(result);
        appService.Verify(x => x.GetInheritancedAppsAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAsync_MapsApplicationTimeCreatorAndInheritance()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        var currentUser = BuildCurrentUser();
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2025, 2, 3, 4, 5, 6, TimeSpan.Zero));
        App addedApp = null;
        List<AppInheritanced> addedInheritance = null;

        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync((App)null);
        appService.Setup(x => x.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>()))
            .Callback<App, List<AppInheritanced>>((app, inheritance) =>
            {
                addedApp = app;
                addedInheritance = inheritance;
            })
            .ReturnsAsync(true);

        var service = new ApplicationManagementService(
            appService.Object,
            eventBus.Object,
            currentUser.Object,
            timeProvider,
            new Mock<IPreviewModeAccessor>().Object);

        var result = await service.CreateAsync(new CreateApplicationCommand(
            "app-1",
            "Application 1",
            "group-a",
            "secret",
            false,
            false,
            new[] { "parent-a", "parent-b" }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(addedApp, result.Value);
        Assert.AreEqual("app-1", addedApp.Id);
        Assert.AreEqual("Application 1", addedApp.Name);
        Assert.AreEqual("group-a", addedApp.Group);
        Assert.AreEqual("secret", addedApp.Secret);
        Assert.IsFalse(addedApp.Enabled);
        Assert.AreEqual(AppType.PRIVATE, addedApp.Type);
        Assert.AreEqual("user-1", addedApp.Creator);
        Assert.AreEqual(timeProvider.GetLocalNow().DateTime, addedApp.CreateTime);
        Assert.AreEqual(2, addedInheritance.Count);
        Assert.AreEqual("parent-a", addedInheritance[0].InheritancedAppId);
        Assert.AreEqual("parent-b", addedInheritance[1].InheritancedAppId);
        Assert.AreEqual(0, addedInheritance[0].Sort);
        Assert.AreEqual(1, addedInheritance[1].Sort);
        Assert.IsTrue(addedInheritance.All(x => x.AppId == "app-1" && !string.IsNullOrWhiteSpace(x.Id)));
        eventBus.Verify(x => x.Fire(It.IsAny<AddAppSuccessful>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAsync_WhenIdentifierExists_ReturnsConflictWithoutWriting()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(new App { Id = "app-1" });

        var service = CreateService(appService, eventBus);
        var result = await service.CreateAsync(new CreateApplicationCommand(
            "app-1", "Application 1", null, null, true, false, Array.Empty<string>()));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        appService.Verify(x => x.AddAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>()), Times.Never);
        eventBus.Verify(x => x.Fire(It.IsAny<DeleteAppSuccessful>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_MapsExistingApplicationAndInheritance()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        var currentUser = BuildCurrentUser();
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2025, 3, 4, 5, 6, 7, TimeSpan.Zero));
        var existing = new App
        {
            Id = "app-1",
            Name = "Old name",
            CreateTime = new DateTime(2020, 1, 1),
            Creator = "original-user"
        };
        App updatedApp = null;
        List<AppInheritanced> updatedInheritance = null;

        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(existing);
        appService.Setup(x => x.UpdateAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>()))
            .Callback<App, List<AppInheritanced>>((app, inheritance) =>
            {
                updatedApp = app;
                updatedInheritance = inheritance;
            })
            .ReturnsAsync(true);

        var service = new ApplicationManagementService(
            appService.Object,
            eventBus.Object,
            currentUser.Object,
            timeProvider,
            new Mock<IPreviewModeAccessor>().Object);

        var result = await service.UpdateAsync(new UpdateApplicationCommand(
            "app-1",
            "New name",
            "group-b",
            "new-secret",
            true,
            true,
            new[] { "ignored-parent" }));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(existing, updatedApp);
        Assert.AreEqual("New name", updatedApp.Name);
        Assert.AreEqual("group-b", updatedApp.Group);
        Assert.AreEqual("new-secret", updatedApp.Secret);
        Assert.IsTrue(updatedApp.Enabled);
        Assert.AreEqual(AppType.Inheritance, updatedApp.Type);
        Assert.AreEqual(new DateTime(2020, 1, 1), updatedApp.CreateTime);
        Assert.AreEqual(timeProvider.GetLocalNow().DateTime, updatedApp.UpdateTime);
        Assert.AreEqual(0, updatedInheritance.Count);
        eventBus.Verify(x => x.Fire(It.IsAny<EditAppSuccessful>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateAsync_InPreviewMode_RejectsDemoApplicationBeforeWriting()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        var previewMode = new Mock<IPreviewModeAccessor>();
        previewMode.SetupGet(x => x.IsPreviewMode).Returns(true);
        appService.Setup(x => x.GetAsync("app-1"))
            .ReturnsAsync(new App { Id = "app-1", Name = "test_app" });

        var service = new ApplicationManagementService(
            appService.Object,
            eventBus.Object,
            BuildCurrentUser().Object,
            TimeProvider.System,
            previewMode.Object);

        var result = await service.UpdateAsync(new UpdateApplicationCommand(
            "app-1", "Changed", null, null, true, false, Array.Empty<string>()));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.ValidationFailed, result.Error);
        appService.Verify(x => x.UpdateAsync(It.IsAny<App>(), It.IsAny<List<AppInheritanced>>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_PublishesDeleteEventAfterSuccessfulDelete()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        var currentUser = BuildCurrentUser();
        var app = new App { Id = "app-1", Name = "Application 1" };
        appService.Setup(x => x.GetAsync("app-1")).ReturnsAsync(app);
        appService.Setup(x => x.DeleteAsync(app)).ReturnsAsync(true);

        var service = new ApplicationManagementService(
            appService.Object,
            eventBus.Object,
            currentUser.Object,
            TimeProvider.System,
            new Mock<IPreviewModeAccessor>().Object);

        var result = await service.DeleteAsync(new DeleteApplicationCommand("app-1"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(app, result.Value);
        eventBus.Verify(x => x.Fire(It.Is<DeleteAppSuccessful>(e =>
            ReferenceEquals(e.App, app) && e.UserName == "alice")), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenApplicationDoesNotExist_ReturnsNotFound()
    {
        var appService = new Mock<IAppService>();
        var eventBus = new Mock<ITinyEventBus>();
        appService.Setup(x => x.GetAsync("missing")).ReturnsAsync((App)null);

        var service = CreateService(appService, eventBus);
        var result = await service.DeleteAsync(new DeleteApplicationCommand("missing"));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        appService.Verify(x => x.DeleteAsync(It.IsAny<App>()), Times.Never);
        eventBus.Verify(x => x.Fire(It.IsAny<DeleteAppSuccessful>()), Times.Never);
    }

    private static ApplicationManagementService CreateService(
        Mock<IAppService> appService,
        Mock<ITinyEventBus> eventBus)
    {
        return new ApplicationManagementService(
            appService.Object,
            eventBus.Object,
            BuildCurrentUser().Object,
            TimeProvider.System,
            new Mock<IPreviewModeAccessor>().Object);
    }

    private static Mock<ICurrentUserAccessor> BuildCurrentUser()
    {
        var currentUser = new Mock<ICurrentUserAccessor>();
        currentUser.SetupGet(x => x.UserName).Returns("alice");
        currentUser.Setup(x => x.GetUserIdAsync()).ReturnsAsync("user-1");
        return currentUser;
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
