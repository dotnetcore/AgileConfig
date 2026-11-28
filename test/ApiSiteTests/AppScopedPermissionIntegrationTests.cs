using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AgileConfig.Server.Application.Users;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests;

[TestClass]
[DoNotParallelize]
public class AppScopedPermissionIntegrationTests
{
    private const string GrantedApp = "team-a-app";
    private const string ForeignApp = "team-b-app";

    [TestMethod]
    public async Task Operators_CanOnlyReachApplicationsTheyCreatedOrWereAuthorizedFor()
    {
        using var host = new V2ApiSiteTestHost();
        foreach (var id in new[] { GrantedApp, ForeignApp })
        {
            var createApp = await host.SendAsync(HttpMethod.Post, "/api/v2/applications",
                new { id, name = id, secret = "secret", enabled = true });
            Assert.AreEqual(HttpStatusCode.Created, createApp.StatusCode);
        }

        var createForeignConfig = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{ForeignApp}/environments/TEST/configurations",
            new { key = "conn", value = "foreign-secret" });
        Assert.AreEqual(HttpStatusCode.Created, createForeignConfig.StatusCode);
        var foreignConfigId = (await V2ApiSiteTestHost.ReadJsonAsync(createForeignConfig)).GetProperty("id").GetString();

        string operatorId;
        using (var scope = host.Services.CreateScope())
        {
            var created = await scope.ServiceProvider.GetRequiredService<IUserManagementService>()
                .CreateAsync(new CreateUserCommand("operator", "pwd", "team-a", [SystemRoleConstants.OperatorId]));
            Assert.IsTrue(created.Succeeded);
            operatorId = created.Value.Id;
            await scope.ServiceProvider.GetRequiredService<IAppService>().SaveUserAppAuth(GrantedApp, [operatorId]);
        }

        var basic = V2ApiSiteTestHost.BasicAuthentication("operator", "pwd");

        async Task<HttpStatusCode> Status(HttpMethod method, string path, object body = null,
            AuthenticationHeaderValue authorization = null)
        {
            return (await host.SendAsync(method, path, body, authorization ?? basic)).StatusCode;
        }

        // Lists only contain the operator's applications.
        foreach (var listPath in new[] { "/api/v2/applications", "/api/app" })
        {
            var list = await V2ApiSiteTestHost.ReadJsonAsync(
                await host.SendAsync(HttpMethod.Get, listPath, authorization: basic));
            var ids = list.EnumerateArray().Select(x => x.GetProperty("id").GetString()).ToList();
            CollectionAssert.AreEqual(new[] { GrantedApp }, ids, listPath);
        }

        // The authorized application stays usable.
        Assert.AreEqual(HttpStatusCode.OK, await Status(HttpMethod.Get, $"/api/v2/applications/{GrantedApp}"));
        Assert.AreEqual(HttpStatusCode.Created, await Status(HttpMethod.Post,
            $"/api/v2/applications/{GrantedApp}/environments/TEST/configurations",
            new { key = "own", value = "own-value" }));

        // Every route into the other application is rejected, including the ones addressed by configuration id.
        var foreignV2 = $"/api/v2/applications/{ForeignApp}";
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Get, foreignV2));
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Delete, foreignV2));
        Assert.AreEqual(HttpStatusCode.Forbidden,
            await Status(HttpMethod.Get, $"{foreignV2}/environments/TEST/configurations"));
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Post,
            $"{foreignV2}/environments/TEST/configurations", new { key = "injected", value = "x" }));
        Assert.AreEqual(HttpStatusCode.Forbidden,
            await Status(HttpMethod.Get, $"/api/config?appId={ForeignApp}&env=TEST"));
        Assert.AreEqual(HttpStatusCode.Forbidden,
            await Status(HttpMethod.Get, $"/api/config/{foreignConfigId}?env=TEST"));
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Put,
            $"/api/config/{foreignConfigId}?env=TEST", new { appId = GrantedApp, key = "conn", value = "tampered" }));
        Assert.AreEqual(HttpStatusCode.Forbidden,
            await Status(HttpMethod.Delete, $"/api/config/{foreignConfigId}?env=TEST"));

        // The admin console endpoints enforce the same rule for a signed-in operator.
        var login = await V2ApiSiteTestHost.ReadJsonAsync(await host.SendAsync(HttpMethod.Post, "/admin/jwt/login",
            new { userName = "operator", password = "pwd" }));
        var bearer = new AuthenticationHeaderValue("Bearer", login.GetProperty("token").GetString());

        Assert.AreEqual(HttpStatusCode.OK, await Status(HttpMethod.Get,
            $"/config/search?appId={GrantedApp}&env=TEST", authorization: bearer));
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Get,
            $"/config/search?appId={ForeignApp}&env=TEST", authorization: bearer));
        Assert.AreEqual(HttpStatusCode.Forbidden, await Status(HttpMethod.Get,
            $"/app/get?id={ForeignApp}", authorization: bearer));

        var all = await V2ApiSiteTestHost.ReadJsonAsync(
            await host.SendAsync(HttpMethod.Get, "/config/all?env=TEST", authorization: bearer));
        var allAppIds = all.GetProperty("data").EnumerateArray().Select(x => x.GetProperty("appId").GetString()).ToList();
        CollectionAssert.AreEqual(new[] { GrantedApp }, allAppIds);

        // Being authorized for an app is not enough to hand it out or to read another app through inheritance.
        var saveAuth = await V2ApiSiteTestHost.ReadJsonAsync(await host.SendAsync(HttpMethod.Post, "/app/saveAppAuth",
            new { appId = GrantedApp, authorizedUsers = new[] { operatorId } }, bearer));
        Assert.IsFalse(saveAuth.GetProperty("success").GetBoolean());

        var inherit = await V2ApiSiteTestHost.ReadJsonAsync(await host.SendAsync(HttpMethod.Post, "/app/add",
            new { id = "team-a-child", name = "team-a-child", enabled = true, inheritancedApps = new[] { ForeignApp } },
            bearer));
        Assert.IsFalse(inherit.GetProperty("success").GetBoolean());

        // Nothing leaked into, or was changed in, the other application.
        var foreignConfig = await V2ApiSiteTestHost.ReadJsonAsync(
            await host.SendAsync(HttpMethod.Get, $"/api/config/{foreignConfigId}?env=TEST"));
        Assert.AreEqual("foreign-secret", foreignConfig.GetProperty("value").GetString());
    }
}
