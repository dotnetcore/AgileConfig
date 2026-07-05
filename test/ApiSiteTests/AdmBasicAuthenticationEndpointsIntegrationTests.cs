using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Abstraction;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests;

[TestClass]
public class AdmBasicAuthenticationEndpointsIntegrationTests
{
    [TestMethod]
    public async Task AdmBasicAuthenticationEndpoints_ShouldBeAvailable()
    {
        using var host = new ApiSiteTestHost();
        var appId = $"adm-api-{Guid.NewGuid():N}";
        var appName = $"Adm API {Guid.NewGuid():N}";
        var configKey = $"key-{Guid.NewGuid():N}";

        var createApp = await host.SendAsync(HttpMethod.Post, "/api/app", new
        {
            id = appId,
            name = appName,
            secret = "secret-1",
            enabled = true,
            group = "integration"
        });
        Assert.AreEqual(HttpStatusCode.Created, createApp.StatusCode);

        var appList = await host.SendAsync(HttpMethod.Get, "/api/app");
        Assert.AreEqual(HttpStatusCode.OK, appList.StatusCode);
        var appListJson = await ReadJsonAsync(appList);
        Assert.IsTrue(appListJson.EnumerateArray().Any(x => x.GetProperty("id").GetString() == appId));

        var appById = await host.SendAsync(HttpMethod.Get, $"/api/app/{appId}");
        Assert.AreEqual(HttpStatusCode.OK, appById.StatusCode);
        var appByIdJson = await ReadJsonAsync(appById);
        Assert.AreEqual(appId, appByIdJson.GetProperty("id").GetString());

        var editApp = await host.SendAsync(HttpMethod.Put, $"/api/app/{appId}", new
        {
            id = appId,
            name = appName + " updated",
            secret = "secret-2",
            enabled = true,
            group = "integration-updated"
        });
        Assert.AreEqual(HttpStatusCode.OK, editApp.StatusCode);

        var emptyConfigs = await host.SendAsync(HttpMethod.Get, $"/api/config?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, emptyConfigs.StatusCode);
        var emptyConfigsJson = await ReadJsonAsync(emptyConfigs);
        Assert.AreEqual(0, emptyConfigsJson.GetArrayLength());

        var addConfig = await host.SendAsync(HttpMethod.Post, "/api/config?env=TEST", new
        {
            appId,
            key = configKey,
            value = "v1",
            group = "grp",
            description = "first"
        });
        Assert.AreEqual(HttpStatusCode.Created, addConfig.StatusCode);

        var configList = await host.SendAsync(HttpMethod.Get, $"/api/config?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, configList.StatusCode);
        var configListJson = await ReadJsonAsync(configList);
        Assert.AreEqual(1, configListJson.GetArrayLength());
        var configId = configListJson[0].GetProperty("id").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(configId));

        var configById = await host.SendAsync(HttpMethod.Get, $"/api/config/{configId}?env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, configById.StatusCode);
        var configByIdJson = await ReadJsonAsync(configById);
        Assert.AreEqual("v1", configByIdJson.GetProperty("value").GetString());

        var editConfig = await host.SendAsync(HttpMethod.Put, $"/api/config/{configId}?env=TEST", new
        {
            appId,
            key = configKey,
            value = "v2",
            group = "grp",
            description = "second"
        });
        Assert.AreEqual(HttpStatusCode.OK, editConfig.StatusCode);

        var publishFirst = await host.SendAsync(HttpMethod.Post, $"/api/app/publish?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, publishFirst.StatusCode);

        var historyAfterFirstPublish = await host.SendAsync(HttpMethod.Get,
            $"/api/app/Publish_History?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, historyAfterFirstPublish.StatusCode);
        var historyAfterFirstPublishJson = await ReadJsonAsync(historyAfterFirstPublish);
        Assert.AreEqual(1, historyAfterFirstPublishJson.GetArrayLength());

        var editConfigAgain = await host.SendAsync(HttpMethod.Put, $"/api/config/{configId}?env=TEST", new
        {
            appId,
            key = configKey,
            value = "v3",
            group = "grp",
            description = "third"
        });
        Assert.AreEqual(HttpStatusCode.OK, editConfigAgain.StatusCode);

        var publishSecond = await host.SendAsync(HttpMethod.Post, $"/api/app/publish?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, publishSecond.StatusCode);

        var historyAfterSecondPublish = await host.SendAsync(HttpMethod.Get,
            $"/api/app/Publish_History?appId={appId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, historyAfterSecondPublish.StatusCode);
        var historyAfterSecondPublishJson = await ReadJsonAsync(historyAfterSecondPublish);
        Assert.IsTrue(historyAfterSecondPublishJson.GetArrayLength() >= 2);
        var rollbackId = historyAfterSecondPublishJson.EnumerateArray()
            .OrderBy(x => x.GetProperty("version").GetInt32())
            .First()
            .GetProperty("id")
            .GetString();

        var rollback = await host.SendAsync(HttpMethod.Post, $"/api/app/rollback?historyId={rollbackId}&env=TEST");
        Assert.AreEqual(HttpStatusCode.OK, rollback.StatusCode);

        var nodeList = await host.SendAsync(HttpMethod.Get, "/api/node");
        Assert.AreEqual(HttpStatusCode.OK, nodeList.StatusCode);

        var nodeAddress = $"http://127.0.0.1:{GetPort()}";
        var addNode = await host.SendAsync(HttpMethod.Post, "/api/node", new
        {
            address = nodeAddress,
            remark = "integration-node"
        });
        Assert.AreEqual(HttpStatusCode.Created, addNode.StatusCode);

        var deleteNode = await host.SendAsync(HttpMethod.Delete,
            $"/api/node?address={Uri.EscapeDataString(nodeAddress)}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteNode.StatusCode);

        var deleteConfig = await host.SendAsync(HttpMethod.Delete, $"/api/config/{configId}?env=TEST");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteConfig.StatusCode);

        var deleteApp = await host.SendAsync(HttpMethod.Delete, $"/api/app/{appId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteApp.StatusCode);
    }

    [TestMethod]
    public async Task AdmBasicAuthenticationEndpoints_ShouldRejectInvalidCredentials()
    {
        using var host = new ApiSiteTestHost();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/app");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:wrong")));

        var response = await host.Client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task TryInitSystemRolesAndPermissions_RepeatedCalls_DoNotDuplicateBindings()
    {
        using var host = new ApiSiteTestHost();

        // The host constructor already ran system initialization once.
        var baseline = await host.CountSuperAdminBindingsAsync();
        Assert.IsTrue(baseline > 0,
            "SuperAdministrator should have permission bindings after the first initialization.");

        // Simulate repeated startups invoking the initialization again.
        for (var i = 0; i < 3; i++)
        {
            var ok = await host.RunSystemRolesAndPermissionsInitAsync();
            Assert.IsTrue(ok, "Repeated initialization should succeed.");
        }

        var (total, distinct) = await host.CountSuperAdminBindingsDistinctAsync();
        Assert.AreEqual(baseline, total,
            "Repeated initialization must not increase the number of role-function bindings.");
        Assert.AreEqual(total, distinct,
            "SuperAdministrator must not have duplicated function bindings.");
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.Clone();
    }

    private static int GetPort()
    {
        return 20000 + Random.Shared.Next(10000);
    }

    private sealed class ApiSiteTestHost : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly TestServer _server;

        public ApiSiteTestHost()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "AgileConfig.ApiSiteTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);

            var dbPath = Path.Combine(_tempDirectory, "agile_config.db");
            var testDbPath = Path.Combine(_tempDirectory, "agile_config_test.db");
            var contentRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                "src", "AgileConfig.Server.Apisite"));

            var settings = new Dictionary<string, string>
            {
                ["urls"] = "http://127.0.0.1:0",
                ["adminConsole"] = "true",
                ["saPassword"] = "1",
                ["defaultApp"] = "",
                ["cluster"] = "false",
                ["preview_mode"] = "false",
                ["alwaysTrustSsl"] = "false",
                ["serviceHealthCheckInterval"] = "60",
                ["serviceUnhealthInterval"] = "60",
                ["removeServiceInterval"] = "0",
                ["pathBase"] = "",
                ["db:provider"] = "sqlite",
                ["db:conn"] = $"Data Source={dbPath}",
                ["db:env:TEST:provider"] = "sqlite",
                ["db:env:TEST:conn"] = $"Data Source={testDbPath}",
                ["otlp:instanceId"] = "",
                ["otlp:logs:endpoint"] = "",
                ["otlp:logs:headers"] = "",
                ["otlp:logs:protocol"] = "http",
                ["otlp:traces:endpoint"] = "",
                ["otlp:traces:headers"] = "",
                ["otlp:traces:protocol"] = "http",
                ["otlp:metrics:endpoint"] = "",
                ["otlp:metrics:headers"] = "",
                ["otlp:metrics:protocol"] = "http",
                ["SSO:enabled"] = "false",
                ["SSO:loginButtonText"] = "",
                ["JwtSetting:Issuer"] = "agileconfig.admin",
                ["JwtSetting:Audience"] = "agileconfig.admin",
                ["JwtSetting:ExpireSeconds"] = "86400"
            };

            Global.Config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            Global.LoggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning));

            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(contentRoot)
                .UseConfiguration(Global.Config)
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseStartup<Startup>();

            _server = new TestServer(builder);
            InitializeSystemData();
            Client = _server.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:1")));
        }

        public HttpClient Client { get; }

        private void InitializeSystemData()
        {
            var freeSqlFactory = _server.Services.GetRequiredService<IFreeSqlFactory>();
            var initializationService = _server.Services.GetRequiredService<ISystemInitializationService>();

            _ = freeSqlFactory.Create();
            _ = freeSqlFactory.Create("TEST");

            initializationService.TryInitDefaultEnvironment();
            initializationService.TryInitJwtSecret();
            initializationService.TryInitSystemRolesAndPermissions().GetAwaiter().GetResult();
            initializationService.TryInitSaPassword();
        }

        public async Task<bool> RunSystemRolesAndPermissionsInitAsync()
        {
            using var scope = _server.Services.CreateScope();
            var initializationService = scope.ServiceProvider.GetRequiredService<ISystemInitializationService>();
            return await initializationService.TryInitSystemRolesAndPermissions();
        }

        public async Task<int> CountSuperAdminBindingsAsync()
        {
            using var scope = _server.Services.CreateScope();
            var roleFunctionRepository = scope.ServiceProvider.GetRequiredService<IRoleFunctionRepository>();
            var bindings = await roleFunctionRepository.QueryAsync(x => x.RoleId == SystemRoleConstants.SuperAdminId);
            return bindings.Count;
        }

        public async Task<(int total, int distinct)> CountSuperAdminBindingsDistinctAsync()
        {
            using var scope = _server.Services.CreateScope();
            var roleFunctionRepository = scope.ServiceProvider.GetRequiredService<IRoleFunctionRepository>();
            var bindings = await roleFunctionRepository.QueryAsync(x => x.RoleId == SystemRoleConstants.SuperAdminId);
            var total = bindings.Count;
            var distinct = bindings.Select(x => x.FunctionId).Distinct().Count();
            return (total, distinct);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object body = null)
        {
            using var request = new HttpRequestMessage(method, path);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await Client.SendAsync(request);
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();

            try
            {
                if (Directory.Exists(_tempDirectory)) Directory.Delete(_tempDirectory, true);
            }
            catch
            {
            }
        }
    }
}