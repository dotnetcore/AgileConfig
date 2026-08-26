using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AgileConfig.Server.OIDC;
using AgileConfig.Server.OIDC.SettingProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Common.Tests;

[TestClass]
public class OidcClientTests
{
    [TestMethod]
    public async Task Validate_UsesConfiguredAuthenticationMethodsAndReadsIdToken()
    {
        var setting = CreateSetting("client_secret_basic");
        var basicHandler = new RecordingHandler();
        var client = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(basicHandler)));

        var token = await client.Validate("code-1");

        Assert.AreEqual("token-value", token.IdToken);
        Assert.AreEqual("Basic Y2xpZW50OnNlY3JldA==", basicHandler.Request.Headers.Authorization.ToString());
        StringAssert.Contains(basicHandler.Body, "code=code-1");
        Assert.DoesNotContain("client_secret", basicHandler.Body);

        setting.TokenEndpointAuthMethod = "client_secret_post";
        var postHandler = new RecordingHandler();
        client = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(postHandler)));
        await client.Validate("code-2");
        Assert.IsNull(postHandler.Request.Headers.Authorization);
        StringAssert.Contains(postHandler.Body, "client_id=client");
        StringAssert.Contains(postHandler.Body, "client_secret=secret");

        setting.TokenEndpointAuthMethod = "none";
        var noneHandler = new RecordingHandler();
        client = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(noneHandler)));
        await client.Validate("code-3");
        Assert.IsNull(noneHandler.Request.Headers.Authorization);
        Assert.DoesNotContain("client_id", noneHandler.Body);
    }

    [TestMethod]
    public void GetAuthorizeUrl_UsesConfiguredClientParameters()
    {
        var setting = CreateSetting("none");
        var client = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(new RecordingHandler())));

        var authorizeUrl = client.GetAuthorizeUrl();
        StringAssert.StartsWith(authorizeUrl, "https://issuer.test/authorize?");
        StringAssert.Contains(authorizeUrl, "client_id=client");
        StringAssert.Contains(authorizeUrl, "redirect_uri=https://app.test/callback");

    }

    [TestMethod]
    public async Task Validate_RejectsEmptyAndIncompleteResponses()
    {
        var setting = CreateSetting("none");
        var emptyClient = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(new RecordingHandler(""))));
        await Assert.ThrowsExactlyAsync<Exception>(() => emptyClient.Validate("code"));

        var missingTokenClient = new OidcClient(new StaticSettingProvider(setting), new StaticHttpClientFactory(new HttpClient(new RecordingHandler("{}"))));
        await Assert.ThrowsExactlyAsync<Exception>(() => missingTokenClient.Validate("code"));
    }

    [TestMethod]
    public void SettingsProviderAndServiceRegistration_ReadConfiguration()
    {
        Global.Config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["SSO:OIDC:clientId"] = "configured-client",
            ["SSO:OIDC:clientSecret"] = "configured-secret",
            ["SSO:OIDC:redirectUri"] = "https://configured/callback",
            ["SSO:OIDC:tokenEndpoint"] = "https://configured/token",
            ["SSO:OIDC:authorizationEndpoint"] = "https://configured/authorize",
            ["SSO:OIDC:userIdClaim"] = "id",
            ["SSO:OIDC:userNameClaim"] = "username",
            ["SSO:OIDC:scope"] = "openid profile",
            ["SSO:OIDC:tokenEndpointAuthMethod"] = "none"
        }).Build();
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var provider = new ConfigfileOidcSettingProvider(loggerFactory.CreateLogger<ConfigfileOidcSettingProvider>());

        Assert.AreEqual("configured-client", provider.GetSetting().ClientId);

        var services = new ServiceCollection();
        services.AddOIDC();
        Assert.IsTrue(services.Any(x => x.ServiceType == typeof(IOidcClient) && x.Lifetime == ServiceLifetime.Singleton));
        Assert.IsTrue(services.Any(x => x.ServiceType == typeof(IOidcSettingProvider) && x.Lifetime == ServiceLifetime.Singleton));
    }

    private static OidcSetting CreateSetting(string method) => new(
        "client", "secret", "https://app.test/callback", "https://issuer.test/token", "https://issuer.test/authorize",
        "sub", "name", "openid profile", method);

    private sealed class StaticSettingProvider(OidcSetting setting) : IOidcSettingProvider
    {
        public OidcSetting GetSetting() => setting;
    }

    private sealed class StaticHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(string response = "{\"id_token\":\"token-value\"}") : HttpMessageHandler
    {
        public HttpRequestMessage Request { get; private set; }
        public string Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) };
        }
    }
}
