using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Controllers.api.v2.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests;

[TestClass]
[DoNotParallelize]
public class V2RestApiEndpointsIntegrationTests
{
    [TestMethod]
    public async Task ApplicationsConfigurationsAndReleases_ShouldFollowRestSemantics()
    {
        using var host = new V2ApiSiteTestHost();
        var applicationId = $"v2-{Guid.NewGuid():N}";

        var createApplication = await host.SendAsync(HttpMethod.Post, "/api/v2/applications", new
        {
            id = applicationId,
            name = "V2 integration application",
            group = "integration",
            secret = "client-secret",
            enabled = true,
            isInheritanceSource = false,
            inheritsFrom = Array.Empty<string>()
        });
        Assert.AreEqual(HttpStatusCode.Created, createApplication.StatusCode);
        Assert.AreEqual($"/api/v2/applications/{applicationId}", createApplication.Headers.Location?.PathAndQuery);
        var application = await V2ApiSiteTestHost.ReadJsonAsync(createApplication);
        Assert.AreEqual(applicationId, application.GetProperty("id").GetString());
        Assert.IsFalse(application.TryGetProperty("secret", out _));

        var duplicateApplication = await host.SendAsync(HttpMethod.Post, "/api/v2/applications", new
        {
            id = applicationId,
            name = "Duplicate"
        });
        await AssertProblem(duplicateApplication, HttpStatusCode.Conflict);

        var applicationList = await host.SendAsync(HttpMethod.Get, "/api/v2/applications");
        Assert.AreEqual(HttpStatusCode.OK, applicationList.StatusCode);
        var applications = await V2ApiSiteTestHost.ReadJsonAsync(applicationList);
        Assert.IsTrue(applications.EnumerateArray().Any(x => x.GetProperty("id").GetString() == applicationId));

        var updateApplication = await host.SendAsync(HttpMethod.Put, $"/api/v2/applications/{applicationId}", new
        {
            name = "V2 integration application updated",
            group = "integration-updated",
            secret = "client-secret",
            enabled = true,
            isInheritanceSource = false,
            inheritsFrom = Array.Empty<string>()
        });
        Assert.AreEqual(HttpStatusCode.OK, updateApplication.StatusCode);
        var updatedApplication = await V2ApiSiteTestHost.ReadJsonAsync(updateApplication);
        Assert.AreEqual("integration-updated", updatedApplication.GetProperty("group").GetString());

        var createConfiguration = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations", new
            {
                group = "database",
                key = "connectionString",
                value = "v1",
                description = "first value"
            });
        Assert.AreEqual(HttpStatusCode.Created, createConfiguration.StatusCode);
        var configuration = await V2ApiSiteTestHost.ReadJsonAsync(createConfiguration);
        var configurationId = configuration.GetProperty("id").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(configurationId));
        StringAssert.Contains(createConfiguration.Headers.Location?.OriginalString, $"/configurations/{configurationId}");

        var duplicateConfiguration = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations", new
            {
                group = "database",
                key = "connectionString",
                value = "duplicate"
            });
        await AssertProblem(duplicateConfiguration, HttpStatusCode.Conflict);

        var firstRelease = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases", new
            {
                log = "first release"
            });
        Assert.AreEqual(HttpStatusCode.Created, firstRelease.StatusCode);
        var firstReleaseJson = await V2ApiSiteTestHost.ReadJsonAsync(firstRelease);
        var firstReleaseId = firstReleaseJson.GetProperty("id").GetString();
        Assert.AreEqual(1, firstReleaseJson.GetProperty("version").GetInt32());

        var updateConfiguration = await host.SendAsync(HttpMethod.Put,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations/{configurationId}", new
            {
                group = "database",
                key = "connectionString",
                value = "v2",
                description = "second value"
            });
        Assert.AreEqual(HttpStatusCode.OK, updateConfiguration.StatusCode);
        var updatedConfiguration = await V2ApiSiteTestHost.ReadJsonAsync(updateConfiguration);
        Assert.AreEqual("v2", updatedConfiguration.GetProperty("value").GetString());
        Assert.AreEqual("second value", updatedConfiguration.GetProperty("description").GetString());

        var secondRelease = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases", new
            {
                log = "second release",
                configurationIds = new[] { configurationId }
            });
        Assert.AreEqual(HttpStatusCode.Created, secondRelease.StatusCode);

        var releasesResponse = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases");
        Assert.AreEqual(HttpStatusCode.OK, releasesResponse.StatusCode);
        var releases = await V2ApiSiteTestHost.ReadJsonAsync(releasesResponse);
        Assert.IsTrue(releases.GetArrayLength() >= 2);

        var releaseResponse = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases/{firstReleaseId}");
        Assert.AreEqual(HttpStatusCode.OK, releaseResponse.StatusCode);

        var rollback = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases/rollbacks", new
            {
                releaseId = firstReleaseId
            });
        Assert.AreEqual(HttpStatusCode.NoContent, rollback.StatusCode);

        var deleteConfiguration = await host.SendAsync(HttpMethod.Delete,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations/{configurationId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteConfiguration.StatusCode);

        var missingConfiguration = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations/{configurationId}");
        Assert.AreEqual(HttpStatusCode.NotFound, missingConfiguration.StatusCode);

        var deleteApplication = await host.SendAsync(HttpMethod.Delete, $"/api/v2/applications/{applicationId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteApplication.StatusCode);

        var missingApplication = await host.SendAsync(HttpMethod.Get, $"/api/v2/applications/{applicationId}");
        Assert.AreEqual(HttpStatusCode.NotFound, missingApplication.StatusCode);

        var legacyApi = await host.SendAsync(HttpMethod.Get, "/api/app");
        Assert.AreEqual(HttpStatusCode.OK, legacyApi.StatusCode,
            "The legacy REST API must remain available after adding v2 routes.");
    }

    [TestMethod]
    public async Task PublishedConfigurations_ShouldUseApplicationAuthenticationAndEtags()
    {
        using var host = new V2ApiSiteTestHost();
        var applicationId = $"p-{Guid.NewGuid():N}";
        await CreateApplicationAndPublishedConfiguration(host, applicationId, "pull-secret");

        var applicationAuth = V2ApiSiteTestHost.BasicAuthentication(applicationId, "pull-secret");
        var response = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/published-configurations",
            authorization: applicationAuth);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Headers.ETag != null);
        Assert.IsTrue(response.Headers.Contains("X-Publish-Timeline-Id"));
        var configurations = await V2ApiSiteTestHost.ReadJsonAsync(response);
        Assert.AreEqual(1, configurations.GetArrayLength());
        Assert.AreEqual("published-value", configurations[0].GetProperty("value").GetString());
        Assert.AreEqual(applicationId, configurations[0].GetProperty("applicationId").GetString());
        Assert.AreEqual("TEST", configurations[0].GetProperty("environment").GetString());

        var notModified = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/published-configurations",
            authorization: applicationAuth,
            etag: response.Headers.ETag.Tag);
        Assert.AreEqual(HttpStatusCode.NotModified, notModified.StatusCode);

        var matchingList = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/published-configurations",
            authorization: applicationAuth,
            etag: $"\"stale\", W/{response.Headers.ETag.Tag}");
        Assert.AreEqual(HttpStatusCode.NotModified, matchingList.StatusCode);

        var wildcard = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/{applicationId}/environments/TEST/published-configurations",
            authorization: applicationAuth,
            etag: "*");
        Assert.AreEqual(HttpStatusCode.NotModified, wildcard.StatusCode);

        var mismatch = await host.SendAsync(HttpMethod.Get,
            $"/api/v2/applications/another-app/environments/TEST/published-configurations",
            authorization: applicationAuth);
        await AssertProblem(mismatch, HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task Nodes_ShouldUseOpaqueIdentifiersAndStandardStatuses()
    {
        using var host = new V2ApiSiteTestHost();
        var address = $"http://127.0.0.1:{Random.Shared.Next(20000, 30000)}";

        var create = await host.SendAsync(HttpMethod.Post, "/api/v2/nodes", new
        {
            address,
            remark = "v2 integration node"
        });
        Assert.AreEqual(HttpStatusCode.Created, create.StatusCode);
        var node = await V2ApiSiteTestHost.ReadJsonAsync(create);
        var nodeId = node.GetProperty("id").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(nodeId));
        Assert.IsFalse(nodeId.Contains('/'));

        var get = await host.SendAsync(HttpMethod.Get, $"/api/v2/nodes/{nodeId}");
        Assert.AreEqual(HttpStatusCode.OK, get.StatusCode);

        var duplicate = await host.SendAsync(HttpMethod.Post, "/api/v2/nodes", new { address });
        await AssertProblem(duplicate, HttpStatusCode.Conflict);

        var delete = await host.SendAsync(HttpMethod.Delete, $"/api/v2/nodes/{nodeId}");
        Assert.AreEqual(HttpStatusCode.NoContent, delete.StatusCode);

        var missing = await host.SendAsync(HttpMethod.Get, $"/api/v2/nodes/{nodeId}");
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task ServiceInstances_ShouldSupportRegistrationHeartbeatFilteringAndDeletion()
    {
        using var host = new V2ApiSiteTestHost();
        var serviceId = $"service-{Guid.NewGuid():N}";
        var request = new
        {
            serviceId,
            name = "V2 service",
            ipAddress = "127.0.0.1",
            port = 8080,
            metadata = new[] { "zone=local" },
            heartbeatMode = "client",
            healthCheckUrl = "http://127.0.0.1:8080/health",
            alarmUrl = ""
        };

        var oversizedMetadata = await host.SendAsync(HttpMethod.Post, "/api/v2/service-instances", new
        {
            serviceId = $"oversized-{Guid.NewGuid():N}",
            name = "V2 service",
            ipAddress = "127.0.0.1",
            port = 8080,
            metadata = new[] { new string('x', RegisterServiceInstanceRequest.MetadataMaxLength - 3) },
            heartbeatMode = "client",
            healthCheckUrl = "",
            alarmUrl = ""
        });
        await AssertProblem(oversizedMetadata, HttpStatusCode.BadRequest);

        var register = await host.SendAsync(HttpMethod.Post, "/api/v2/service-instances", request);
        Assert.AreEqual(HttpStatusCode.Created, register.StatusCode);
        var instance = await V2ApiSiteTestHost.ReadJsonAsync(register);
        var instanceId = instance.GetProperty("id").GetString();
        Assert.IsFalse(string.IsNullOrWhiteSpace(instanceId));

        var get = await host.SendAsync(HttpMethod.Get, $"/api/v2/service-instances/{instanceId}");
        Assert.AreEqual(HttpStatusCode.OK, get.StatusCode);

        var healthy = await host.SendAsync(HttpMethod.Get, "/api/v2/service-instances?status=Healthy");
        Assert.AreEqual(HttpStatusCode.OK, healthy.StatusCode);
        var healthyInstances = await V2ApiSiteTestHost.ReadJsonAsync(healthy);
        Assert.IsTrue(healthyInstances.EnumerateArray().Any(x => x.GetProperty("id").GetString() == instanceId));

        var heartbeat = await host.SendAsync(HttpMethod.Put,
            $"/api/v2/service-instances/{instanceId}/heartbeat");
        Assert.AreEqual(HttpStatusCode.OK, heartbeat.StatusCode);
        var heartbeatJson = await V2ApiSiteTestHost.ReadJsonAsync(heartbeat);
        Assert.AreEqual(instanceId, heartbeatJson.GetProperty("serviceInstanceId").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(heartbeatJson.GetProperty("servicesVersion").GetString()));

        var updateRegistration = await host.SendAsync(HttpMethod.Post, "/api/v2/service-instances", request);
        Assert.AreEqual(HttpStatusCode.OK, updateRegistration.StatusCode);
        var updatedInstance = await V2ApiSiteTestHost.ReadJsonAsync(updateRegistration);
        Assert.AreEqual(instanceId, updatedInstance.GetProperty("id").GetString());

        var delete = await host.SendAsync(HttpMethod.Delete, $"/api/v2/service-instances/{instanceId}");
        Assert.AreEqual(HttpStatusCode.NoContent, delete.StatusCode);

        var missing = await host.SendAsync(HttpMethod.Put,
            $"/api/v2/service-instances/{instanceId}/heartbeat");
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [TestMethod]
    public async Task V2AdminEndpoints_ShouldRejectInvalidCredentialsAndInvalidModels()
    {
        using var host = new V2ApiSiteTestHost();

        var invalidCredentials = await host.SendAsync(HttpMethod.Get, "/api/v2/applications",
            authorization: V2ApiSiteTestHost.BasicAuthentication("admin", "wrong"));
        Assert.AreEqual(HttpStatusCode.Forbidden, invalidCredentials.StatusCode);

        var invalidModel = await host.SendAsync(HttpMethod.Post, "/api/v2/applications", new
        {
            name = "Missing identifier"
        });
        await AssertProblem(invalidModel, HttpStatusCode.BadRequest);

        var invalidInheritance = await host.SendAsync(HttpMethod.Post, "/api/v2/applications", new
        {
            id = $"child-{Guid.NewGuid():N}",
            name = "Invalid inheritance application",
            inheritsFrom = new[] { "missing-inheritance-source" }
        });
        await AssertProblem(invalidInheritance, HttpStatusCode.BadRequest);

        var missing = await host.SendAsync(HttpMethod.Get, "/api/v2/applications/does-not-exist");
        Assert.AreEqual(HttpStatusCode.NotFound, missing.StatusCode);

        var unknownEnvironment = await host.SendAsync(HttpMethod.Get,
            "/api/v2/applications/does-not-exist/environments/UNKNOWN/configurations");
        await AssertProblem(unknownEnvironment, HttpStatusCode.BadRequest);
    }

    private static async Task CreateApplicationAndPublishedConfiguration(
        V2ApiSiteTestHost host,
        string applicationId,
        string secret)
    {
        var application = await host.SendAsync(HttpMethod.Post, "/api/v2/applications", new
        {
            id = applicationId,
            name = "Published config application",
            secret,
            enabled = true
        });
        Assert.AreEqual(HttpStatusCode.Created, application.StatusCode);

        var configuration = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/configurations", new
            {
                key = "published-key",
                value = "published-value"
            });
        Assert.AreEqual(HttpStatusCode.Created, configuration.StatusCode);

        var release = await host.SendAsync(HttpMethod.Post,
            $"/api/v2/applications/{applicationId}/environments/TEST/releases", new { log = "publish" });
        Assert.AreEqual(HttpStatusCode.Created, release.StatusCode);
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        Assert.AreEqual(expectedStatus, response.StatusCode);
        StringAssert.Contains(response.Content.Headers.ContentType?.MediaType, "problem");
        var problem = await V2ApiSiteTestHost.ReadJsonAsync(response);
        Assert.AreEqual((int)expectedStatus, problem.GetProperty("status").GetInt32());
    }
}
