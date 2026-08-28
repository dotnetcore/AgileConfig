using System;
using System.Collections.Generic;
using System.IO;
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

namespace ApiSiteTests;

internal sealed class V2ApiSiteTestHost : IDisposable
{
    private readonly string _tempDirectory;
    private readonly TestServer _server;

    public V2ApiSiteTestHost(bool previewMode = false)
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "AgileConfig.V2ApiSiteTests", Guid.NewGuid().ToString("N"));
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
            ["preview_mode"] = previewMode.ToString(),
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
        Client.DefaultRequestHeaders.Authorization = BasicAuthentication("admin", "1");
    }

    public HttpClient Client { get; }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object body = null,
        AuthenticationHeaderValue authorization = null,
        string etag = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (authorization != null) request.Headers.Authorization = authorization;
        if (etag != null) request.Headers.TryAddWithoutValidation("If-None-Match", etag);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await Client.SendAsync(request);
    }

    public static AuthenticationHeaderValue BasicAuthentication(string username, string password)
    {
        return new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
    }

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        return document.RootElement.Clone();
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
}
