using System;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Application;
using AgileConfig.Server.Application.Configurations;
using AgileConfig.Server.Apisite.Controllers.api.v2;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using AgileConfig.Server.Data.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests;

[TestClass]
public sealed class V2ControllerErrorMappingTests
{
    [TestMethod]
    public async Task DeleteNode_WhenForbidden_Returns403()
    {
        var service = new Mock<INodeManagementService>();
        service.Setup(x => x.DeleteAsync("http://node", default))
            .ReturnsAsync(ApplicationResult.Failure(ApplicationError.Forbidden));
        var controller = Initialize(new NodesController(service.Object));
        var nodeId = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("http://node"));

        var result = await controller.Delete(nodeId);

        Assert.AreEqual(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
    }

    [TestMethod]
    public async Task UpdateApplication_WhenPersistenceFails_Returns500()
    {
        var service = new Mock<IApplicationManagementService>();
        service.Setup(x => x.UpdateAsync(It.IsAny<UpdateApplicationCommand>()))
            .ReturnsAsync(ApplicationResult<App>.Failure(ApplicationError.OperationFailed));
        var controller = Initialize(new ApplicationsController(service.Object));

        var result = await controller.Update("app-1", new UpdateApplicationRequest
        {
            Name = "Application",
            Enabled = true
        });

        Assert.AreEqual(
            StatusCodes.Status500InternalServerError,
            ((ObjectResult)result.Result).StatusCode);
    }

    [TestMethod]
    public async Task UpdateConfiguration_WhenPersistenceFails_Returns500()
    {
        var applications = new Mock<IApplicationManagementService>();
        var configurations = new Mock<IConfigurationManagementService>();
        configurations.Setup(x => x.GetAsync("app-1", "TEST", "config-1"))
            .ReturnsAsync(new Config { Id = "config-1", AppId = "app-1", Env = "TEST" });
        configurations.Setup(x => x.UpdateAsync(It.IsAny<UpdateConfigurationCommand>()))
            .ReturnsAsync(ApplicationResult<Config>.Failure(ApplicationError.OperationFailed));
        var controller = Initialize(new ConfigurationsController(applications.Object, configurations.Object));

        var result = await controller.Update("app-1", "TEST", "config-1", new UpdateConfigurationRequest
        {
            Group = "default",
            Key = "key",
            Value = "value"
        });

        Assert.AreEqual(
            StatusCodes.Status500InternalServerError,
            ((ObjectResult)result.Result).StatusCode);
    }

    private static T Initialize<T>(T controller) where T : ControllerBase
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = services.BuildServiceProvider()
            }
        };
        return controller;
    }
}
