using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AgileConfig.Server.ApplicationTests;

[TestClass]
public sealed class PublishedConfigurationQueryServiceTests
{
    [TestMethod]
    public async Task GetAsync_ReturnsTimelineAndInheritedPublishedConfigurations()
    {
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        var configurations = new List<Config> { new() { Id = "config-1" } };
        appService.Setup(x => x.GetAsync("app-1"))
            .ReturnsAsync(new App { Id = "app-1", Enabled = true });
        configService.Setup(x => x.GetLastPublishTimelineVirtualIdAsyncWithCache("app-1", "PROD"))
            .ReturnsAsync("timeline-1");
        configService.Setup(x => x.GetPublishedConfigsByAppIdWithInheritance("app-1", "PROD"))
            .ReturnsAsync(configurations);
        var service = new PublishedConfigurationQueryService(appService.Object, configService.Object);

        var result = await service.GetAsync("app-1", "PROD");

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual("timeline-1", result.Value.PublishTimelineId);
        Assert.AreSame(configurations, result.Value.Configurations);
    }

    [TestMethod]
    public async Task GetAsync_WhenApplicationIsDisabled_ReturnsNotFoundWithoutLoadingConfigurations()
    {
        var appService = new Mock<IAppService>();
        var configService = new Mock<IConfigService>();
        appService.Setup(x => x.GetAsync("app-1"))
            .ReturnsAsync(new App { Id = "app-1", Enabled = false });
        var service = new PublishedConfigurationQueryService(appService.Object, configService.Object);

        var result = await service.GetAsync("app-1", "PROD");

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(ApplicationError.NotFound, result.Error);
        configService.Verify(
            x => x.GetPublishedConfigsByAppIdWithInheritance(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
