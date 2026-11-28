using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ApiSiteTests.Filters;

[TestClass]
public class PermissionCheckAttributeTests
{
    private const string UserId = "user-1";
    private static readonly EnvString Env = new() { Value = "DEV" };

    [TestMethod]
    public async Task AppScope_AllowsOnlyApplicationsTheUserCanAccess()
    {
        Assert.IsTrue(await IsAllowed(AppScope.App, new() { ["appId"] = "app-a" }));
        Assert.IsTrue(await IsAllowed(AppScope.App, new() { ["model"] = new ConfigVM { AppId = "app-a" } }));

        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["appId"] = "app-b" }));
        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["id"] = "app-b" }));
        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["applicationId"] = "app-b" }));
        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["appId"] = "APP-A" }));
        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["model"] = new AppVM { Id = "app-b" } }));
    }

    [TestMethod]
    public async Task AppScope_RejectsRequestsWithoutTargetAndBatchesContainingForeignApplications()
    {
        Assert.IsFalse(await IsAllowed(AppScope.App, new() { ["group"] = "db" }));
        Assert.IsFalse(await IsAllowed(AppScope.App, new()
        {
            ["model"] = new List<ConfigVM> { new() { AppId = "app-a" }, new() { AppId = "app-b" } }
        }));
    }

    [TestMethod]
    public async Task Administrators_AreNotLimitedToAuthorizedApplications()
    {
        Assert.IsTrue(await IsAllowed(AppScope.App, new() { ["appId"] = "app-b" }, UserAppAccess.Admin(UserId)));
        Assert.IsTrue(await IsAllowed(AppScope.Config, new() { ["id"] = "cfg-b", ["env"] = Env },
            UserAppAccess.Admin(UserId)));
    }

    [TestMethod]
    public async Task ConfigScope_ChecksTheApplicationOfEveryStoredConfiguration()
    {
        Assert.IsTrue(await IsAllowed(AppScope.Config, new() { ["id"] = "cfg-a", ["env"] = Env }));
        // Unknown ids reach the action, which reports them as not found.
        Assert.IsTrue(await IsAllowed(AppScope.Config, new() { ["id"] = "missing", ["env"] = Env }));

        Assert.IsFalse(await IsAllowed(AppScope.Config, new() { ["configId"] = "cfg-b", ["env"] = Env }));
        Assert.IsFalse(await IsAllowed(AppScope.Config, new()
        {
            ["ids"] = new List<string> { "cfg-a", "cfg-b" },
            ["env"] = Env
        }));
        // The body names an accessible app, but the configuration being edited belongs to another one.
        Assert.IsFalse(await IsAllowed(AppScope.Config, new()
        {
            ["model"] = new ConfigVM { Id = "cfg-b", AppId = "app-a" },
            ["env"] = Env
        }));
        Assert.IsFalse(await IsAllowed(AppScope.Config, new() { ["id"] = "cfg-a" }));
    }

    [TestMethod]
    public async Task PublishTimelineScope_ChecksTheApplicationOfThePublishRecord()
    {
        Assert.IsTrue(await IsAllowed(AppScope.PublishTimeline,
            new() { ["publishTimelineId"] = "timeline-a", ["env"] = Env }));
        Assert.IsFalse(await IsAllowed(AppScope.PublishTimeline,
            new() { ["historyId"] = "timeline-b", ["env"] = Env }));
    }

    [TestMethod]
    public void AppAndConfigActions_DeclareHowTheyAreLimitedToAnApplication()
    {
        // Actions that create an app, or that limit their result to the caller's apps on their own.
        var exempt = new HashSet<string>
        {
            "Controllers.AppController.Search",
            "Controllers.AppController.Export",
            "Controllers.AppController.InheritancedApps",
            "Controllers.AppController.GetAppGroups",
            "Controllers.api.AppController.Add",
            "Controllers.api.v2.ApplicationsController.Create"
        };

        var unscoped = typeof(Startup).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes<TypeFilterAttribute>().Any(filter =>
                typeof(PermissionCheckAttribute).IsAssignableFrom(filter.ImplementationType) &&
                filter.Arguments?.FirstOrDefault() is string function &&
                (function.StartsWith("APP_") || function.StartsWith("CONFIG_")) &&
                filter.Arguments.OfType<AppScope>().All(scope => scope == AppScope.None)))
            .Select(method => $"{method.DeclaringType!.FullName}.{method.Name}"
                .Replace("AgileConfig.Server.Apisite.", string.Empty))
            .Where(name => !exempt.Contains(name))
            .ToList();

        Assert.IsEmpty(unscoped,
            $"App and config actions must declare an AppScope: {string.Join(", ", unscoped)}");
    }

    private static async Task<bool> IsAllowed(AppScope scope, Dictionary<string, object> arguments,
        UserAppAccess access = null)
    {
        var permissionService = new Mock<IPermissionService>();
        permissionService.Setup(x => x.GetUserPermission(UserId)).ReturnsAsync(new List<string> { "FUNCTION" });
        permissionService.Setup(x => x.GetUserAppAccessAsync(UserId))
            .ReturnsAsync(access ?? UserAppAccess.Restricted(UserId, ["app-a"]));

        var configService = new Mock<IConfigService>();
        configService.Setup(x => x.GetAsync("cfg-a", "DEV")).ReturnsAsync(new Config { Id = "cfg-a", AppId = "app-a" });
        configService.Setup(x => x.GetAsync("cfg-b", "DEV")).ReturnsAsync(new Config { Id = "cfg-b", AppId = "app-b" });
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("timeline-a", "DEV"))
            .ReturnsAsync(new PublishTimeline { Id = "timeline-a", AppId = "app-a" });
        configService.Setup(x => x.GetPublishTimeLineNodeAsync("timeline-b", "DEV"))
            .ReturnsAsync(new PublishTimeline { Id = "timeline-b", AppId = "app-b" });

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("id", UserId)], "test"))
        };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), arguments, null);

        var executed = false;
        var filter = new PermissionCheckAttribute(permissionService.Object, configService.Object, "FUNCTION", scope);
        await filter.OnActionExecutionAsync(context, () =>
        {
            executed = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), null));
        });

        Assert.AreEqual(!executed, httpContext.Response.StatusCode == StatusCodes.Status403Forbidden);
        return executed;
    }
}
