using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Abstraction.DbProvider;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Repository.Selector;
using AgileConfig.Server.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public class DatabaseResilienceTests
{
    [TestMethod]
    public async Task Circuit_RejectsRequestsWhileOpen_AndAllowsOnlyOneRecoveryProbe()
    {
        var time = new AdjustableTimeProvider();
        var circuit = CreateCircuit(time, 3);
        for (var i = 0; i < 3; i++)
        {
            circuit.CreateResource(() => new object());
            await Assert.ThrowsAsync<DatabaseUnavailableException>(() =>
                circuit.ExecuteAsync<int>(() => Task.FromException<int>(new TimeoutException())));
        }
        Assert.Throws<DatabaseUnavailableException>(() => circuit.CreateResource(() => new object()));

        var calls = 0;
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() =>
            circuit.ExecuteAsync(() => Task.FromResult(++calls)));
        Assert.AreEqual(0, calls);

        time.Advance(TimeSpan.FromSeconds(10));
        var probe = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = circuit.ExecuteAsync(() => probe.Task);
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() =>
            circuit.ExecuteAsync(() => Task.FromResult(++calls)));
        probe.SetResult(42);
        Assert.AreEqual(42, await pending);
        Assert.AreEqual(1, await circuit.ExecuteAsync(() => Task.FromResult(++calls)));
    }

    [TestMethod]
    public async Task Circuit_LateSuccessCannotCloseCircuit_AndFailedProbeReopensIt()
    {
        var time = new AdjustableTimeProvider();
        var circuit = CreateCircuit(time);
        var oldRequest = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var oldResult = circuit.ExecuteAsync(() => oldRequest.Task);
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute<int>(() => throw new TimeoutException()));
        oldRequest.SetResult(1);
        await oldResult;
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute(() => 2));

        time.Advance(TimeSpan.FromSeconds(10));
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute<int>(() => throw new TimeoutException()));
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute(() => 2));
        time.Advance(TimeSpan.FromSeconds(10));
        Assert.AreEqual(2, circuit.Execute(() => 2));
    }

    [TestMethod]
    public async Task Circuit_DoesNotTreatQueryErrorsOrCancellationAsOutages()
    {
        var circuit = CreateCircuit(new AdjustableTimeProvider());
        Assert.Throws<QueryException>(() => circuit.Execute<int>(() => throw new QueryException()));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            circuit.ExecuteAsync<int>(() => Task.FromCanceled<int>(new CancellationToken(true))));
        Assert.AreEqual(0L, circuit.FailureVersion);
        Assert.AreEqual(3, circuit.Execute(() => 3));
    }

    [TestMethod]
    public void Classifier_DistinguishesSqliteUnavailabilityFromConstraintErrors()
    {
        Assert.IsTrue(DatabaseCircuitRegistry.IsConnectivityFailure(
            new System.Data.SQLite.SQLiteException(System.Data.SQLite.SQLiteErrorCode.Busy, "locked")));
        Assert.IsFalse(DatabaseCircuitRegistry.IsConnectivityFailure(
            new System.Data.SQLite.SQLiteException(System.Data.SQLite.SQLiteErrorCode.Constraint, "duplicate")));
    }

    [TestMethod]
    public async Task Publishing_ReleasesItsLockWhenTheCircuitRejectsUnitOfWorkCreation()
    {
        var service = new ConfigService(null, null, null, null,
            _ => throw new DatabaseUnavailableException(TimeSpan.FromSeconds(10)),
            null, null, null, null);
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() => service.Publish("app", null, "", "", "DEV"));
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() =>
            service.Publish("app", null, "", "", "DEV").WaitAsync(TimeSpan.FromSeconds(2)));
    }

    [TestMethod]
    public void Circuit_TracksProviderConstructionFailuresWithoutTreatingCachedResourcesAsRecovery()
    {
        var time = new AdjustableTimeProvider();
        var circuit = CreateCircuit(time);
        Assert.Throws<DatabaseUnavailableException>(() =>
            circuit.CreateResource<object>(() => throw new TimeoutException()));
        Assert.Throws<DatabaseUnavailableException>(() => circuit.Execute(() => 1));
        time.Advance(TimeSpan.FromSeconds(10));
        circuit.CreateResource(() => new object());
        Assert.AreEqual(1L, circuit.FailureVersion);
        Assert.AreEqual(1, circuit.Execute(() => 1));
    }

    [TestMethod]
    public void Registry_IsolatesDifferentDatabases_AndSharesDefaultEnvironmentCircuit()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["db:provider"] = "sqlite",
            ["db:conn"] = "Data Source=default.db",
            ["db:env:TEST:provider"] = "sqlite",
            ["db:env:TEST:conn"] = "Data Source=test.db",
            ["databaseResilience:failureThreshold"] = "1"
        }).Build();
        var registry = new DatabaseCircuitRegistry(new DbConfigInfoFactory(config),
            new AdjustableTimeProvider(), NullLogger<DatabaseCircuitBreaker>.Instance, config);
        var defaultCircuit = registry.Get();
        Assert.Throws<DatabaseUnavailableException>(() =>
            defaultCircuit.Execute<int>(() => throw new TimeoutException()));
        Assert.Throws<DatabaseUnavailableException>(() => registry.Get("DEV").Execute(() => 1));
        Assert.AreEqual(1, registry.Get("TEST").Execute(() => 1));
    }

    [TestMethod]
    public async Task RepositoryProxy_CoversSyncAsyncAndVoidOperations_AndDisposesWhileOpen()
    {
        var time = new AdjustableTimeProvider();
        var circuit = CreateCircuit(time);
        var repository = new Mock<ISettingRepository>();
        repository.Setup(x => x.GetAsync("settings")).ReturnsAsync(new Setting { Id = "settings" });
        repository.Setup(x => x.UpdateAsync(It.IsAny<Setting>())).ThrowsAsync(new TimeoutException());
        var proxy = (ISettingRepository)ResilientRepositoryProxy.Wrap(typeof(ISettingRepository),
            () => repository.Object, circuit);
        Assert.AreEqual("settings", (await proxy.GetAsync("settings")).Id);
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() => proxy.UpdateAsync(new Setting()));
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() => proxy.GetAsync("settings"));
        proxy.Dispose();
        repository.Verify(x => x.Dispose(), Times.Once);
        repository.Verify(x => x.GetAsync("settings"), Times.Once);

        time.Advance(TimeSpan.FromSeconds(10));
        var init = new Mock<ISysInitRepository>();
        init.Setup(x => x.GetDefaultEnvironmentFromDb()).Returns("DEV,PROD");
        var initProxy = (ISysInitRepository)ResilientRepositoryProxy.Wrap(typeof(ISysInitRepository),
            () => init.Object, circuit);
        Assert.AreEqual("DEV,PROD", initProxy.GetDefaultEnvironmentFromDb());
    }

    [TestMethod]
    public async Task RepositoryProxy_RetriesFailedConstruction_InsteadOfCachingTheException()
    {
        var time = new AdjustableTimeProvider();
        var circuit = CreateCircuit(time);
        var repository = new Mock<ISettingRepository>();
        repository.Setup(x => x.AllAsync()).ReturnsAsync(new List<Setting>());
        var constructions = 0;
        var proxy = (ISettingRepository)ResilientRepositoryProxy.Wrap(typeof(ISettingRepository), () =>
        {
            if (++constructions == 1) throw new TimeoutException();
            return repository.Object;
        }, circuit);
        await Assert.ThrowsAsync<DatabaseUnavailableException>(() => proxy.AllAsync());
        time.Advance(TimeSpan.FromSeconds(10));
        Assert.IsEmpty(await proxy.AllAsync());
        Assert.AreEqual(2, constructions);
    }

    [TestMethod]
    public async Task Middleware_RejectsBeforeInitialization_AndReportsOutagesWithoutExposingProviderDetails()
    {
        var state = new InitializationState();
        var calls = 0;
        var middleware = new DatabaseAvailabilityMiddleware(_ =>
        {
            calls++;
            throw new DatabaseUnavailableException(TimeSpan.FromSeconds(10), new TimeoutException("private details"));
        }, state, NullLogger<DatabaseAvailabilityMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        Assert.AreEqual(503, context.Response.StatusCode);
        Assert.AreEqual(0, calls);

        state.MarkReady();
        context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        Assert.AreEqual(503, context.Response.StatusCode);
        Assert.AreEqual("10", context.Response.Headers.RetryAfter.ToString());
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("private details", body);
        Assert.AreEqual(1, calls);
    }

    internal static DatabaseCircuitBreaker CreateCircuit(TimeProvider time, int threshold = 1) =>
        new(time, NullLogger<DatabaseCircuitBreaker>.Instance, DatabaseCircuitRegistry.IsConnectivityFailure,
            threshold, TimeSpan.FromSeconds(10));

    private sealed class QueryException : DbException;
}

internal class AdjustableTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    public void Advance(TimeSpan duration) => _now += duration;
}
