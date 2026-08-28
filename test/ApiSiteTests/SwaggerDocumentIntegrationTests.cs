using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiSiteTests;

[TestClass]
public sealed class SwaggerDocumentIntegrationTests
{
    [TestMethod]
    public async Task SwaggerDocuments_SeparateV1AndV2Routes()
    {
        using var host = new V2ApiSiteTestHost(previewMode: true);

        using var v1Response = await host.Client.GetAsync("/swagger/v1/swagger.json");
        using var v2Response = await host.Client.GetAsync("/swagger/v2/swagger.json");

        Assert.AreEqual(HttpStatusCode.OK, v1Response.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, v2Response.StatusCode);

        using var v1 = JsonDocument.Parse(await v1Response.Content.ReadAsStringAsync());
        using var v2 = JsonDocument.Parse(await v2Response.Content.ReadAsStringAsync());
        var v1Paths = v1.RootElement.GetProperty("paths").EnumerateObject().Select(x => x.Name).ToList();
        var v2Paths = v2.RootElement.GetProperty("paths").EnumerateObject().Select(x => x.Name).ToList();

        Assert.IsNotEmpty(v1Paths);
        Assert.IsNotEmpty(v2Paths);
        Assert.IsFalse(v1Paths.Any(path => path.StartsWith("/api/v2/")));
        Assert.IsTrue(v2Paths.All(path => path.StartsWith("/api/v2/")));
        Assert.Contains("/api/v2/applications", v2Paths);
    }
}
