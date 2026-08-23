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
public sealed class ServiceInstanceManagementServiceTests
{
    [TestMethod]
    public async Task RegisterAsync_RejectsExistingServiceWhenRequested()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        var existing = new ServiceInfo { Id = "existing", ServiceId = "orders" };
        serviceInfo.Setup(x => x.GetByServiceIdAsync("orders")).ReturnsAsync(existing);

        var registerCenter = new Mock<IRegisterCenterService>();
        var eventBus = new Mock<ITinyEventBus>();
        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);

        var result = await service.RegisterAsync(CreateCommand(rejectExisting: true));

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.Conflict, result.Error);
        registerCenter.Verify(x => x.RegisterAsync(It.IsAny<ServiceInfo>()), Times.Never);
        eventBus.Verify(x => x.Fire(It.IsAny<ServiceRegisteredEvent>()), Times.Never);
    }

    [TestMethod]
    public async Task RegisterAsync_BuildsEntityRegistersPublishesEventAndLoadsInstance()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByServiceIdAsync("orders"))
            .ReturnsAsync((ServiceInfo)null);
        var registered = new ServiceInfo
        {
            Id = "instance-1",
            ServiceId = "orders",
            ServiceName = "Orders",
            MetaData = "[\"zone-a\"]"
        };
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("instance-1")).ReturnsAsync(registered);

        var registerCenter = new Mock<IRegisterCenterService>();
        ServiceInfo captured = null;
        registerCenter.Setup(x => x.RegisterAsync(It.IsAny<ServiceInfo>()))
            .Callback<ServiceInfo>(x => captured = x)
            .ReturnsAsync("instance-1");
        var eventBus = new Mock<ITinyEventBus>();

        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);

        var result = await service.RegisterAsync(CreateCommand(rejectExisting: false));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("instance-1", result.Value.UniqueId);
        Assert.AreSame(registered, result.Value.Instance);
        Assert.IsFalse(result.Value.WasExisting);
        Assert.AreEqual("orders", captured.ServiceId);
        Assert.AreEqual("Orders", captured.ServiceName);
        Assert.AreEqual("10.0.0.5", captured.Ip);
        Assert.AreEqual(8080, captured.Port);
        Assert.AreEqual("[\"zone-a\"]", captured.MetaData);
        Assert.AreEqual(RegisterWay.Auto, captured.RegisterWay);
        eventBus.Verify(x => x.Fire(It.Is<ServiceRegisteredEvent>(e => e.UniqueId == "instance-1")), Times.Once);
    }

    [TestMethod]
    public async Task RegisterAsync_AllowsExistingServiceAndReportsUpdate()
    {
        var old = new ServiceInfo { Id = "instance-1", ServiceId = "orders" };
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByServiceIdAsync("orders")).ReturnsAsync(old);
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("instance-1")).ReturnsAsync(old);
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.RegisterAsync(It.IsAny<ServiceInfo>())).ReturnsAsync("instance-1");

        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            Mock.Of<ITinyEventBus>(),
            TimeProvider.System);

        var result = await service.RegisterAsync(CreateCommand(rejectExisting: false));

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(result.Value.WasExisting);
    }

    [TestMethod]
    public async Task RegisterAsync_WhenReadbackLags_ReturnsTheRegisteredEntity()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByServiceIdAsync("orders")).ReturnsAsync((ServiceInfo)null);
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("instance-1")).ReturnsAsync((ServiceInfo)null);
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.RegisterAsync(It.IsAny<ServiceInfo>())).ReturnsAsync("instance-1");
        var eventBus = new Mock<ITinyEventBus>();
        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);

        var result = await service.RegisterAsync(CreateCommand(rejectExisting: false));

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("instance-1", result.Value.Instance.Id);
        Assert.AreEqual("orders", result.Value.Instance.ServiceId);
        eventBus.Verify(x => x.Fire(It.Is<ServiceRegisteredEvent>(e => e.UniqueId == "instance-1")), Times.Once);
    }

    [TestMethod]
    public async Task RegisterAsync_AcceptsMissingIdentifierOnlyForLegacyCompatibility()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByServiceIdAsync("orders")).ReturnsAsync((ServiceInfo)null);
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.RegisterAsync(It.IsAny<ServiceInfo>())).ReturnsAsync((string)null);
        var eventBus = new Mock<ITinyEventBus>();
        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);
        var command = CreateCommand(rejectExisting: false) with { AcceptMissingIdentifier = true };

        var result = await service.RegisterAsync(command);

        Assert.IsTrue(result.Succeeded);
        Assert.IsNull(result.Value.UniqueId);
        eventBus.Verify(x => x.Fire(It.Is<ServiceRegisteredEvent>(e => e.UniqueId == null)), Times.Once);
    }

    [TestMethod]
    public async Task UnregisterAsync_UsesFallbackAndPublishesEvent()
    {
        var instance = new ServiceInfo { Id = "instance-1", ServiceId = "orders" };
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("instance-1")).ReturnsAsync(instance);
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.UnRegisterAsync("instance-1")).ReturnsAsync(false);
        registerCenter.Setup(x => x.UnRegisterByServiceIdAsync("orders")).ReturnsAsync(true);
        var eventBus = new Mock<ITinyEventBus>();

        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);

        var result = await service.UnregisterAsync("instance-1", "orders");

        Assert.IsTrue(result.Succeeded);
        registerCenter.Verify(x => x.UnRegisterByServiceIdAsync("orders"), Times.Once);
        eventBus.Verify(x => x.Fire(It.Is<ServiceUnRegisterEvent>(e => e.UniqueId == "instance-1")), Times.Once);
    }

    [TestMethod]
    public async Task UnregisterAsync_ReturnsNotFoundWithoutCallingRegisterCenter()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("missing")).ReturnsAsync((ServiceInfo)null);
        var registerCenter = new Mock<IRegisterCenterService>();

        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            Mock.Of<ITinyEventBus>(),
            TimeProvider.System);

        var result = await service.UnregisterAsync("missing");

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        registerCenter.Verify(x => x.UnRegisterAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task UnregisterAsync_CanPreserveLegacySuccessAndNotificationOnRemovalFailure()
    {
        var instance = new ServiceInfo { Id = "instance-1", ServiceId = "orders" };
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetByUniqueIdAsync("instance-1")).ReturnsAsync(instance);
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.UnRegisterAsync("instance-1")).ReturnsAsync(false);
        var eventBus = new Mock<ITinyEventBus>();
        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            eventBus.Object,
            TimeProvider.System);

        var result = await service.UnregisterAsync("instance-1", succeedWhenRemovalFails: true);

        Assert.IsTrue(result.Succeeded);
        eventBus.Verify(x => x.Fire(It.Is<ServiceUnRegisterEvent>(e => e.UniqueId == "instance-1")), Times.Once);
    }

    [TestMethod]
    public async Task ReceiveHeartbeatAsync_ReturnsReceiptAndServiceVersion()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.ServicesMD5Cache()).ReturnsAsync("version-1");
        var registerCenter = new Mock<IRegisterCenterService>();
        registerCenter.Setup(x => x.ReceiveHeartbeatAsync("instance-1")).ReturnsAsync(true);
        var instant = new DateTimeOffset(2026, 8, 24, 1, 2, 3, TimeSpan.Zero);

        var service = new ServiceInstanceManagementService(
            registerCenter.Object,
            serviceInfo.Object,
            Mock.Of<ITinyEventBus>(),
            new FixedTimeProvider(instant));

        var result = await service.ReceiveHeartbeatAsync("instance-1");

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("instance-1", result.Value.ServiceInstanceId);
        Assert.AreEqual(instant.UtcDateTime, result.Value.ReceivedAt);
        Assert.AreEqual("version-1", result.Value.ServicesVersion);
    }

    [TestMethod]
    public async Task GetAllAsync_SelectsTheRequestedStatusQuery()
    {
        var serviceInfo = new Mock<IServiceInfoService>();
        serviceInfo.Setup(x => x.GetOnlineServiceInfoAsync()).ReturnsAsync(new List<ServiceInfo>());
        serviceInfo.Setup(x => x.GetOfflineServiceInfoAsync()).ReturnsAsync(new List<ServiceInfo>());
        serviceInfo.Setup(x => x.GetAllServiceInfoAsync()).ReturnsAsync(new List<ServiceInfo>());

        var service = new ServiceInstanceManagementService(
            Mock.Of<IRegisterCenterService>(),
            serviceInfo.Object,
            Mock.Of<ITinyEventBus>(),
            TimeProvider.System);

        await service.GetAllAsync(ServiceStatus.Healthy);
        await service.GetAllAsync(ServiceStatus.Unhealthy);
        await service.GetAllAsync();

        serviceInfo.Verify(x => x.GetOnlineServiceInfoAsync(), Times.Once);
        serviceInfo.Verify(x => x.GetOfflineServiceInfoAsync(), Times.Once);
        serviceInfo.Verify(x => x.GetAllServiceInfoAsync(), Times.Once);
    }

    private static RegisterServiceInstanceCommand CreateCommand(bool rejectExisting)
    {
        return new RegisterServiceInstanceCommand(
            "orders",
            "Orders",
            "10.0.0.5",
            8080,
            "[\"zone-a\"]",
            "https://orders/health",
            "https://orders/alarm",
            "client",
            RegisterWay.Auto,
            rejectExisting);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
