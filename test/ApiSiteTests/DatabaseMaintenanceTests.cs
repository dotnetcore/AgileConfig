using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Apisite.Metrics;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.IService;
using AgileConfig.Server.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
[DoNotParallelize]
public class DatabaseMaintenanceTests
{
    private IConfiguration _originalConfig;
    private string[] _originalEnvironments;

    [TestInitialize]
    public void Initialize()
    {
        _originalConfig = Global.Config;
        _originalEnvironments = ISettingService.EnvironmentList;
        Global.Config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["adminConsole"] = "true",
            ["cluster"] = "false",
            ["serviceUnhealthInterval"] = "60",
            ["serviceHealthCheckRecoveryGraceSeconds"] = "60",
            ["removeServiceInterval"] = "120"
        }).Build();
    }

    [TestCleanup]
    public void Cleanup()
    {
        Global.Config = _originalConfig;
        ISettingService.EnvironmentList = _originalEnvironments;
    }

    [TestMethod]
    public async Task NodeWorker_RetriesFailedRounds_UsesFreshScopes_AndStopsInFlightWork()
    {
        var state = new InitializationState();
        state.MarkReady();
        var secondRound = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var rounds = 0;
        var disposedScopes = 0;
        var services = new ServiceCollection();
        services.AddScoped(_ => new DisposalMarker(() => Interlocked.Increment(ref disposedScopes)));
        services.AddScoped(_ =>
        {
            _.GetRequiredService<DisposalMarker>();
            var round = Interlocked.Increment(ref rounds);
            var proxy = new Mock<IRemoteServerNodeProxy>();
            proxy.Setup(x => x.TestEchoAsync(It.IsAny<CancellationToken>())).Returns(async (CancellationToken token) =>
            {
                if (round == 1) throw new TimeoutException();
                secondRound.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            });
            return proxy.Object;
        });
        using var provider = services.BuildServiceProvider();
        using var worker = new NodeMonitorService(provider.GetRequiredService<IServiceScopeFactory>(), state,
            new FastRetryTimeProvider(), NullLogger<NodeMonitorService>.Instance);
        await worker.StartAsync(CancellationToken.None);
        await secondRound.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(1, Volatile.Read(ref disposedScopes));
        await worker.StopAsync(CancellationToken.None);
        Assert.AreEqual(2, disposedScopes);
        Assert.AreEqual(2, rounds);
    }

    [TestMethod]
    public async Task Initialization_RetriesExceptionsAndFalseResults_BeforeRegisteringHandlersOnce()
    {
        var initializer = new Mock<ISystemInitializationService>();
        initializer.SetupSequence(x => x.TryInitDefaultEnvironment())
            .Throws(new TimeoutException()).Returns(false).Returns(true);
        initializer.Setup(x => x.TryInitJwtSecret()).Returns(true);
        initializer.Setup(x => x.TryInitSystemRolesAndPermissions()).ReturnsAsync(true);
        initializer.Setup(x => x.TryInitSaPassword("")).Returns(true);
        initializer.Setup(x => x.TryInitDefaultApp("")).Returns(true);
        var jwt = new Mock<IJwtService>();
        jwt.Setup(x => x.GetSecurityKey()).Returns("already-persisted-key");
        var events = new Mock<IEventHandlerRegister>();
        var services = new ServiceCollection();
        services.AddSingleton(initializer.Object);
        services.AddSingleton(jwt.Object);
        services.AddSingleton(events.Object);
        using var provider = services.BuildServiceProvider();
        var state = new InitializationState();
        using var worker = new InitService(provider.GetRequiredService<IServiceScopeFactory>(),
            Mock.Of<IMeterService>(), NullLogger<InitService>.Instance, state, new FastRetryTimeProvider());
        await worker.StartAsync(CancellationToken.None);
        await state.Ready.WaitAsync(TimeSpan.FromSeconds(5));
        await worker.StopAsync(CancellationToken.None);
        initializer.Verify(x => x.TryInitDefaultEnvironment(), Times.Exactly(3));
        events.Verify(x => x.Register(), Times.Once);
    }

    [TestMethod]
    public async Task ApiHost_RecoversFromStartupAndRuntimeOutages_WithoutRestartOrAuthenticationBypass()
    {
        var databaseAvailable = 0;
        var attempted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var time = new AdjustableTimeProvider();
        using var host = new V2ApiSiteTestHost(configureServices: services =>
        {
            services.AddSingleton<TimeProvider>(time);
            services.Replace(ServiceDescriptor.Singleton<ISysInitRepository>(sp =>
                (ISysInitRepository)ResilientRepositoryProxy.Wrap(typeof(ISysInitRepository), () =>
                {
                    attempted.TrySetResult();
                    if (Volatile.Read(ref databaseAvailable) == 0) throw new TimeoutException();
                    return new AgileConfig.Server.Data.Repository.Freesql.SysInitRepository(
                        sp.GetRequiredService<IFreeSqlFactory>());
                }, sp.GetRequiredService<DatabaseCircuitRegistry>().Get())));
        }, waitForInitialization: false);
        await attempted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        using var live = await host.Client.GetAsync("/health/live");
        Assert.AreEqual(HttpStatusCode.OK, live.StatusCode);
        using var initializing = await host.Client.GetAsync("/api/v2/applications");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, initializing.StatusCode);
        Volatile.Write(ref databaseAvailable, 1);
        await host.Services.GetRequiredService<InitializationState>().Ready.WaitAsync(TimeSpan.FromSeconds(15));
        using var ready = await host.Client.GetAsync("/api/v2/applications");
        Assert.AreEqual(HttpStatusCode.OK, ready.StatusCode);

        var circuit = host.Services.GetRequiredService<DatabaseCircuitRegistry>().Get();
        for (var i = 0; i < 3; i++)
            Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute<int>(() => throw new TimeoutException()));
        using var unavailable = await host.Client.GetAsync("/api/v2/applications");
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, unavailable.StatusCode);
        time.Advance(TimeSpan.FromSeconds(10));
        using var recovered = await host.Client.GetAsync("/api/v2/applications");
        Assert.AreEqual(HttpStatusCode.OK, recovered.StatusCode);
        using var unauthorized = await host.SendAsync(HttpMethod.Get, "/api/v2/applications",
            authorization: V2ApiSiteTestHost.BasicAuthentication("admin", "wrong-password"));
        Assert.AreEqual(HttpStatusCode.Forbidden, unauthorized.StatusCode);
    }

    [TestMethod]
    public void DefaultEnvironment_DoesNotInventStateWhenPersistenceFails_AndUsesConcurrentWritersValue()
    {
        ISettingService.EnvironmentList = null;
        var repository = new Mock<ISysInitRepository>();
        repository.Setup(x => x.SaveInitSetting(It.IsAny<Setting>())).Throws(new InvalidOperationException());
        var service = new SystemInitializationService(repository.Object, Global.Config,
            NullLogger<SystemInitializationService>.Instance, Mock.Of<IFunctionRepository>());
        Assert.IsFalse(service.TryInitDefaultEnvironment());
        Assert.IsNull(ISettingService.EnvironmentList);

        repository.SetupSequence(x => x.GetDefaultEnvironmentFromDb())
            .Returns((string)null).Returns("DEV,CUSTOM");
        Assert.IsTrue(service.TryInitDefaultEnvironment());
        CollectionAssert.AreEqual(new[] { "DEV", "CUSTOM" }, ISettingService.EnvironmentList);
    }

    [TestMethod]
    public async Task HealthChecks_ProtectStaleHeartbeatsAfterOutage_ThenExpireOnlyClientsThatDidNotRecover()
    {
        var time = new AdjustableTimeProvider();
        var circuit = DatabaseResilienceTests.CreateCircuit(time);
        var recovery = new ServiceHealthCheckRecovery(circuit, time, Global.Config,
            NullLogger<ServiceHealthCheckRecovery>.Instance);
        var services = new Mock<IServiceInfoService>();
        services.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<ServiceInfo, bool>>>()))
            .ThrowsAsync(new TimeoutException());
        var checker = new ServiceHealthCheckService(services.Object, NullLogger<ServiceHealthCheckService>.Instance,
            Mock.Of<IRestClient>(), recovery, time);
        await Assert.ThrowsAsync<TimeoutException>(() => checker.CheckAsync(CancellationToken.None));
        time.Advance(TimeSpan.FromHours(1));

        var lost = Client("lost", time.GetLocalNow().DateTime.AddHours(-1));
        var alive = Client("alive", lost.LastHeartBeat.Value);
        services.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<ServiceInfo, bool>>>()))
            .ReturnsAsync(new List<ServiceInfo> { lost, alive });
        services.Setup(x => x.RemoveAsync(It.IsAny<string>())).ReturnsAsync(true);
        await checker.CheckAsync(CancellationToken.None);
        time.Advance(TimeSpan.FromSeconds(59));
        alive.LastHeartBeat = time.GetLocalNow().DateTime;
        await checker.CheckAsync(CancellationToken.None);
        services.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
        services.Verify(x => x.UpdateServiceStatus(It.IsAny<ServiceInfo>(), ServiceStatus.Unhealthy), Times.Never);

        time.Advance(TimeSpan.FromSeconds(1));
        await checker.CheckAsync(CancellationToken.None);
        services.Verify(x => x.RemoveAsync("lost"), Times.Once);
        services.Verify(x => x.RemoveAsync("alive"), Times.Never);
    }

    [TestMethod]
    public async Task HealthChecks_ObserveOutagesReportedByOtherRequestsBetweenScans()
    {
        var time = new AdjustableTimeProvider();
        var circuit = DatabaseResilienceTests.CreateCircuit(time);
        var recovery = new ServiceHealthCheckRecovery(circuit, time, Global.Config,
            NullLogger<ServiceHealthCheckRecovery>.Instance);
        recovery.IsInGracePeriod();
        time.Advance(TimeSpan.FromSeconds(60));
        Assert.IsFalse(recovery.IsInGracePeriod());
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute<int>(() => throw new TimeoutException()));

        var service = Client("healthy", time.GetLocalNow().DateTime.AddHours(-1));
        var services = new Mock<IServiceInfoService>();
        services.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<ServiceInfo, bool>>>()))
            .ReturnsAsync(new List<ServiceInfo> { service });
        var checker = new ServiceHealthCheckService(services.Object, NullLogger<ServiceHealthCheckService>.Instance,
            Mock.Of<IRestClient>(), recovery, time);
        await checker.CheckAsync(CancellationToken.None);
        services.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
        services.Verify(x => x.UpdateServiceStatus(It.IsAny<ServiceInfo>(), ServiceStatus.Unhealthy), Times.Never);
    }

    [TestMethod]
    public async Task HealthChecks_AwaitStatusWrites_AndDoNotMarkServicesOfflineOnShutdown()
    {
        var time = new AdjustableTimeProvider();
        var recovery = new ServiceHealthCheckRecovery(DatabaseResilienceTests.CreateCircuit(time),
            time, Global.Config, NullLogger<ServiceHealthCheckRecovery>.Instance);
        var service = Client("server", time.GetLocalNow().DateTime);
        service.HeartBeatMode = "server";
        service.CheckUrl = "http://service/health";
        var services = new Mock<IServiceInfoService>();
        services.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<ServiceInfo, bool>>>()))
            .ReturnsAsync(new List<ServiceInfo> { service });
        var write = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var writing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        services.Setup(x => x.UpdateServiceStatus(service, ServiceStatus.Healthy)).Returns(() =>
        {
            writing.TrySetResult();
            return write.Task;
        });
        var http = new Mock<IRestClient>();
        http.Setup(x => x.GetAsync(service.CheckUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK));
        var checker = new ServiceHealthCheckService(services.Object, NullLogger<ServiceHealthCheckService>.Instance,
            http.Object, recovery, time);
        var round = checker.CheckAsync(CancellationToken.None);
        await writing.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsFalse(round.IsCompleted);
        write.SetResult();
        await round;

        using var cancellation = new CancellationTokenSource();
        http.Setup(x => x.GetAsync(service.CheckUrl, It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromCanceled<HttpResponseMessage>(cancellation.Token);
            });
        await Assert.ThrowsAsync<OperationCanceledException>(() => checker.CheckAsync(cancellation.Token));
        services.Verify(x => x.UpdateServiceStatus(It.IsAny<ServiceInfo>(), ServiceStatus.Unhealthy), Times.Never);
    }

    [TestMethod]
    public async Task NodeEcho_DoesNotMarkNodeOfflineWhenShutdownCancelsTheHttpRequest()
    {
        using var cancellation = new CancellationTokenSource();
        var node = new ServerNode { Id = "http://node", Status = NodeStatus.Online };
        var nodes = new Mock<IServerNodeService>();
        nodes.Setup(x => x.GetAllNodesAsync()).ReturnsAsync(new List<ServerNode> { node });
        nodes.Setup(x => x.GetAsync(node.Id)).ReturnsAsync(node);
        var http = new Mock<IRestClient>();
        http.Setup(x => x.GetAsync("http://node/home/echo", It.IsAny<CancellationToken>())).Returns(() =>
        {
            cancellation.Cancel();
            return Task.FromCanceled<HttpResponseMessage>(cancellation.Token);
        });
        var proxy = new RemoteServerNodeProxy(NullLoggerFactory.Instance, http.Object, nodes.Object,
            Mock.Of<ISysLogService>());
        await Assert.ThrowsAsync<OperationCanceledException>(() => proxy.TestEchoAsync(cancellation.Token));
        Assert.AreEqual(NodeStatus.Online, node.Status);
        nodes.Verify(x => x.UpdateAsync(It.IsAny<ServerNode>()), Times.Never);
    }

    private static ServiceInfo Client(string id, DateTime heartbeat) => new()
    {
        Id = id, HeartBeatMode = "client", Status = ServiceStatus.Healthy, LastHeartBeat = heartbeat
    };

    private sealed class FastRetryTimeProvider : TimeProvider
    {
        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period) =>
            System.CreateTimer(callback, state, TimeSpan.FromMilliseconds(1), period);
    }

    private sealed class DisposalMarker(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
