using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.EventHandler;
using AgileConfig.Server.IService;
using AgileConfig.Server.SyncPlugin;
using AgileConfig.Server.SyncPlugin.Contracts;
using AgileConfig.Server.SyncPlugin.Retry;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApiSiteTests;

[TestClass]
public class ConfigSyncEventHandlerTests
{
    private Mock<IConfigService> _configServiceMock;
    private Mock<ILogger<SyncEngine>> _syncEngineLoggerMock;
    private Mock<ILogger<SyncRetryService>> _retryServiceLoggerMock;
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<ISyncPlugin> _pluginMock;
    private SyncEngine _syncEngine;
    private SyncRetryService _retryService;
    private Mock<ILogger<ConfigSyncEventHandler>> _handlerLoggerMock;
    private ConfigSyncEventHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _configServiceMock = new Mock<IConfigService>();
        
        _syncEngineLoggerMock = new Mock<ILogger<SyncEngine>>();
        _syncEngine = new SyncEngine(_syncEngineLoggerMock.Object);
        
        _serviceProviderMock = new Mock<IServiceProvider>();
        _retryServiceLoggerMock = new Mock<ILogger<SyncRetryService>>();
        _retryService = new SyncRetryService(
            _syncEngine,
            _serviceProviderMock.Object,
            _retryServiceLoggerMock.Object);
        
        _handlerLoggerMock = new Mock<ILogger<ConfigSyncEventHandler>>();

        _handler = new ConfigSyncEventHandler(
            _configServiceMock.Object,
            _syncEngine,
            _retryService,
            _handlerLoggerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _syncEngine?.Dispose();
    }

    private void RegisterMockPlugin(bool returnSuccess = true)
    {
        _pluginMock = new Mock<ISyncPlugin>();
        _pluginMock.Setup(p => p.Name).Returns("mock_plugin");
        _pluginMock.Setup(p => p.DisplayName).Returns("Mock Plugin");
        _pluginMock.Setup(p => p.Description).Returns("Mock sync plugin for testing");
        
        _pluginMock.Setup(p => p.InitializeAsync(It.IsAny<SyncPluginConfig>()))
            .ReturnsAsync(new SyncPluginResult { Success = true });
        
        _pluginMock.Setup(p => p.SyncAllAsync(It.IsAny<SyncContext[]>()))
            .ReturnsAsync(new SyncPluginResult 
            { 
                Success = returnSuccess, 
                Message = returnSuccess ? "Success" : "Mock failure" 
            });
        
        _pluginMock.Setup(p => p.HealthCheckAsync())
            .ReturnsAsync(new SyncPluginHealthResult { Healthy = true });
        
        _pluginMock.Setup(p => p.ShutdownAsync())
            .Returns(Task.CompletedTask);

        var config = new SyncPluginConfig
        {
            PluginName = "mock_plugin",
            Enabled = "true",
            Settings = new Dictionary<string, string>()
        };
        
        _syncEngine.RegisterPlugin(_pluginMock.Object, config);
    }

    [TestMethod]
    public async Task Handle_WithValidTimeline_ShouldSyncConfigs()
    {
        // Arrange
        RegisterMockPlugin(returnSuccess: true);
        
        var timeline = new PublishTimeline
        {
            Id = "timeline-1",
            AppId = "app-1",
            Env = "PROD",
            PublishTime = DateTime.Now
        };

        var evt = new PublishConfigSuccessful(timeline, "testuser");

        var publishedConfigs = new List<ConfigPublished>
        {
            new ConfigPublished
            {
                Id = "config-1",
                AppId = "app-1",
                Env = "PROD",
                Key = "db.connection",
                Value = "server=localhost",
                Group = "db"
            },
            new ConfigPublished
            {
                Id = "config-2",
                AppId = "app-1",
                Env = "PROD",
                Key = "cache.enabled",
                Value = "true",
                Group = "cache"
            }
        };

        _configServiceMock
            .Setup(x => x.GetPublishedConfigsAsync("app-1", "PROD"))
            .ReturnsAsync(publishedConfigs);

        // Act
        await _handler.Handle(evt);

        // Assert
        _configServiceMock.Verify(
            x => x.GetPublishedConfigsAsync("app-1", "PROD"),
            Times.Once);

        _pluginMock.Verify(
            p => p.SyncAllAsync(It.Is<SyncContext[]>(ctx =>
                ctx.Length == 2 &&
                ctx.Any(c => c.Key == "db.connection") &&
                ctx.Any(c => c.Key == "cache.enabled"))),
            Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNoPublishedConfigs_ShouldNotSync()
    {
        // Arrange
        RegisterMockPlugin(returnSuccess: true);
        
        var timeline = new PublishTimeline
        {
            Id = "timeline-1",
            AppId = "app-1",
            Env = "PROD",
            PublishTime = DateTime.Now
        };

        var evt = new PublishConfigSuccessful(timeline, "testuser");

        _configServiceMock
            .Setup(x => x.GetPublishedConfigsAsync("app-1", "PROD"))
            .ReturnsAsync(new List<ConfigPublished>());

        // Act
        await _handler.Handle(evt);

        // Assert
        _configServiceMock.Verify(
            x => x.GetPublishedConfigsAsync("app-1", "PROD"),
            Times.Once);

        // Should NOT call sync when no configs
        _pluginMock.Verify(
            p => p.SyncAllAsync(It.IsAny<SyncContext[]>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithNullTimeline_ShouldReturnEarly()
    {
        // Arrange
        RegisterMockPlugin(returnSuccess: true);
        
        var evt = new PublishConfigSuccessful(null!, "testuser");

        // Act
        await _handler.Handle(evt);

        // Assert
        _configServiceMock.Verify(
            x => x.GetPublishedConfigsAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _pluginMock.Verify(
            p => p.SyncAllAsync(It.IsAny<SyncContext[]>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenSyncFails_ShouldRecordFailed()
    {
        // Arrange
        RegisterMockPlugin(returnSuccess: false);
        
        var timeline = new PublishTimeline
        {
            Id = "timeline-1",
            AppId = "app-1",
            Env = "PROD",
            PublishTime = DateTime.Now
        };

        var evt = new PublishConfigSuccessful(timeline, "testuser");

        var publishedConfigs = new List<ConfigPublished>
        {
            new ConfigPublished
            {
                Id = "config-1",
                AppId = "app-1",
                Env = "PROD",
                Key = "db.connection",
                Value = "server=localhost",
                Group = "db"
            }
        };

        _configServiceMock
            .Setup(x => x.GetPublishedConfigsAsync("app-1", "PROD"))
            .ReturnsAsync(publishedConfigs);

        // Clear any initial failed records
        _retryService.ClearFailedRecords();

        // Act
        await _handler.Handle(evt);

        // Assert - verify failed record was created
        var failedRecords = _retryService.GetFailedRecords();
        Assert.AreEqual(1, failedRecords.Count);
        Assert.AreEqual("app-1", failedRecords[0].AppId);
        Assert.AreEqual("PROD", failedRecords[0].Env);
    }

    [TestMethod]
    public async Task Handle_WhenExceptionThrown_ShouldRecordFailed()
    {
        // Arrange
        RegisterMockPlugin(returnSuccess: true);
        
        var timeline = new PublishTimeline
        {
            Id = "timeline-1",
            AppId = "app-1",
            Env = "PROD",
            PublishTime = DateTime.Now
        };

        var evt = new PublishConfigSuccessful(timeline, "testuser");

        _configServiceMock
            .Setup(x => x.GetPublishedConfigsAsync("app-1", "PROD"))
            .ThrowsAsync(new Exception("Database connection error"));

        // Clear any initial failed records
        _retryService.ClearFailedRecords();

        // Act
        await _handler.Handle(evt);

        // Assert - verify failed record was created
        var failedRecords = _retryService.GetFailedRecords();
        Assert.AreEqual(1, failedRecords.Count);
        Assert.AreEqual("app-1", failedRecords[0].AppId);
        Assert.AreEqual("PROD", failedRecords[0].Env);
        Assert.AreEqual("Database connection error", failedRecords[0].LastError);
    }
}
