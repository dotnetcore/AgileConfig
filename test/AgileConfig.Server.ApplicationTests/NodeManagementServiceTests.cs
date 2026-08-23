using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.Application.Tests;

[TestClass]
public sealed class NodeManagementServiceTests
{
    [TestMethod]
    public async Task CreateAsync_NormalizesBuildsPersistsAndEchoesNode()
    {
        var repository = new Mock<IServerNodeService>();
        var proxy = new Mock<IRemoteServerNodeProxy>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new Mock<ICurrentUserAccessor>();
        var preview = new Mock<IPreviewModeAccessor>();
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 8, 24, 1, 2, 3, TimeSpan.Zero));
        ServerNode persisted = null;

        repository.Setup(x => x.GetAsync("https://node.example")).ReturnsAsync((ServerNode)null);
        repository.Setup(x => x.AddAsync(It.IsAny<ServerNode>()))
            .Callback<ServerNode>(x => persisted = x)
            .ReturnsAsync(true);
        proxy.Setup(x => x.TestEchoAsync("https://node.example")).Returns(Task.CompletedTask);
        user.SetupGet(x => x.UserName).Returns("operator");

        var service = new NodeManagementService(
            repository.Object,
            proxy.Object,
            eventBus.Object,
            user.Object,
            preview.Object,
            timeProvider);

        var result = await service.CreateAsync(new CreateNodeCommand("https://node.example///", "primary"));

        Assert.IsTrue(result.Succeeded);
        Assert.AreSame(persisted, result.Value);
        Assert.AreEqual("https://node.example", persisted.Id);
        Assert.AreEqual("primary", persisted.Remark);
        Assert.AreEqual(NodeStatus.Offline, persisted.Status);
        Assert.AreEqual(timeProvider.GetLocalNow().DateTime, persisted.CreateTime);
        repository.Verify(x => x.AddAsync(It.IsAny<ServerNode>()), Times.Once);
        proxy.Verify(x => x.TestEchoAsync("https://node.example"), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<AddNodeSuccessful>(e =>
            e.Node == persisted && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_ReturnsConflictWithoutSideEffectsWhenAddressExists()
    {
        var repository = new Mock<IServerNodeService>();
        var proxy = new Mock<IRemoteServerNodeProxy>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new Mock<ICurrentUserAccessor>();
        var preview = new Mock<IPreviewModeAccessor>();
        repository.Setup(x => x.GetAsync("https://node.example"))
            .ReturnsAsync(new ServerNode { Id = "https://node.example" });

        var service = new NodeManagementService(
            repository.Object,
            proxy.Object,
            eventBus.Object,
            user.Object,
            preview.Object,
            TimeProvider.System);

        var result = await service.CreateAsync(new CreateNodeCommand("https://node.example/", null));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        repository.Verify(x => x.AddAsync(It.IsAny<ServerNode>()), Times.Never);
        proxy.Verify(x => x.TestEchoAsync(It.IsAny<string>()), Times.Never);
        eventBus.Verify(x => x.Fire(It.IsAny<AddNodeSuccessful>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_RespectsPreviewModeAndMissingNodes()
    {
        var repository = new Mock<IServerNodeService>();
        var proxy = new Mock<IRemoteServerNodeProxy>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new Mock<ICurrentUserAccessor>();
        var preview = new Mock<IPreviewModeAccessor>();
        preview.SetupGet(x => x.IsPreviewMode).Returns(true);

        var service = new NodeManagementService(
            repository.Object,
            proxy.Object,
            eventBus.Object,
            user.Object,
            preview.Object,
            TimeProvider.System);

        var previewResult = await service.DeleteAsync("https://node.example");

        Assert.IsFalse(previewResult.Succeeded);
        Assert.AreEqual(ApplicationError.Forbidden, previewResult.Error);
        repository.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);

        preview.SetupGet(x => x.IsPreviewMode).Returns(false);
        repository.Setup(x => x.GetAsync("https://missing.example"))
            .ReturnsAsync((ServerNode)null);

        var missingResult = await service.DeleteAsync("https://missing.example/");

        Assert.IsFalse(missingResult.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, missingResult.Error);
        repository.Verify(x => x.DeleteAsync(It.IsAny<ServerNode>()), Times.Never);
    }

    [TestMethod]
    public async Task DeleteAsync_DeletesAndPublishesDomainEvent()
    {
        var repository = new Mock<IServerNodeService>();
        var proxy = new Mock<IRemoteServerNodeProxy>();
        var eventBus = new Mock<ITinyEventBus>();
        var user = new Mock<ICurrentUserAccessor>();
        var preview = new Mock<IPreviewModeAccessor>();
        var node = new ServerNode { Id = "https://node.example" };
        user.SetupGet(x => x.UserName).Returns("operator");
        repository.Setup(x => x.GetAsync(node.Id)).ReturnsAsync(node);
        repository.Setup(x => x.DeleteAsync(node)).ReturnsAsync(true);

        var service = new NodeManagementService(
            repository.Object,
            proxy.Object,
            eventBus.Object,
            user.Object,
            preview.Object,
            TimeProvider.System);

        var result = await service.DeleteAsync("https://node.example/");

        Assert.IsTrue(result.Succeeded);
        repository.Verify(x => x.DeleteAsync(node), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<DeleteNodeSuccessful>(e =>
            e.Node == node && e.UserName == "operator")), Times.Once);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsNodesInCreationOrder()
    {
        var repository = new Mock<IServerNodeService>();
        repository.Setup(x => x.GetAllNodesAsync()).ReturnsAsync(new List<ServerNode>
        {
            new() { Id = "second", CreateTime = new DateTime(2026, 1, 2) },
            new() { Id = "first", CreateTime = new DateTime(2026, 1, 1) }
        });

        var service = new NodeManagementService(
            repository.Object,
            Mock.Of<IRemoteServerNodeProxy>(),
            Mock.Of<ITinyEventBus>(),
            Mock.Of<ICurrentUserAccessor>(),
            Mock.Of<IPreviewModeAccessor>(),
            TimeProvider.System);

        var nodes = await service.GetAllAsync();

        Assert.AreEqual("first", nodes[0].Id);
        Assert.AreEqual("second", nodes[1].Id);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
