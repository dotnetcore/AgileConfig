using AgileConfig.Server.Data.Mongodb;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.MongodbTests;

[TestClass]
public class MongodbTlsOptionsTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [TestMethod]
    public void FromConfiguration_ReturnsNull_WhenNothingIsConfigured()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:provider"] = "mongodb",
            ["db:conn"] = "mongodb://localhost:27017"
        });

        Assert.IsNull(MongodbTlsOptions.FromConfiguration(configuration));
        Assert.IsNull(MongodbTlsOptions.FromConfiguration(configuration, "TEST"));
    }

    [TestMethod]
    public void FromConfiguration_ReturnsNull_WhenConfigurationIsNull()
    {
        Assert.IsNull(MongodbTlsOptions.FromConfiguration(null));
    }

    [TestMethod]
    public void FromConfiguration_ReadsGlobalSettings()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:caFile"] = "/certs/ca.crt",
            ["db:tls:allowInvalidHostnames"] = "true",
            ["db:tls:checkCertificateRevocation"] = "false"
        });

        var options = MongodbTlsOptions.FromConfiguration(configuration);

        Assert.IsNotNull(options);
        Assert.AreEqual("/certs/ca.crt", options.CaFile);
        Assert.IsNull(options.Ca);
        Assert.IsTrue(options.AllowInvalidHostnames);
        Assert.IsFalse(options.AllowInvalidCertificates);
        Assert.IsFalse(options.CheckCertificateRevocation);
    }

    [TestMethod]
    public void FromConfiguration_ReadsInlineCa()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:ca"] = "-----BEGIN CERTIFICATE-----\nMIIB\n-----END CERTIFICATE-----"
        });

        var options = MongodbTlsOptions.FromConfiguration(configuration);

        Assert.IsNotNull(options);
        StringAssert.Contains(options.Ca, "BEGIN CERTIFICATE");
    }

    [TestMethod]
    public void FromConfiguration_EnvironmentSpecificSettingOverridesOnlyThatKey()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:caFile"] = "/certs/ca.crt",
            ["db:tls:allowInvalidHostnames"] = "false",
            ["db:env:TEST:tls:allowInvalidHostnames"] = "true"
        });

        var options = MongodbTlsOptions.FromConfiguration(configuration, "TEST");

        Assert.IsNotNull(options);
        Assert.AreEqual("/certs/ca.crt", options.CaFile);
        Assert.IsTrue(options.AllowInvalidHostnames);
    }

    [TestMethod]
    public void FromConfiguration_EnvironmentWithoutOwnSection_KeepsGlobalSettings()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:caFile"] = "/certs/ca.crt"
        });

        var options = MongodbTlsOptions.FromConfiguration(configuration, "PROD");

        Assert.IsNotNull(options);
        Assert.AreEqual("/certs/ca.crt", options.CaFile);
    }

    [TestMethod]
    public void FromConfiguration_SupportsYesNoAndOneZero()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:allowInvalidCertificates"] = "yes",
            ["db:tls:allowInvalidHostnames"] = "0"
        });

        var options = MongodbTlsOptions.FromConfiguration(configuration);

        Assert.IsNotNull(options);
        Assert.IsTrue(options.AllowInvalidCertificates);
        Assert.IsFalse(options.AllowInvalidHostnames);
    }

    [TestMethod]
    public void FromConfiguration_Throws_ForInvalidBoolean()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string>
        {
            ["db:tls:allowInvalidHostnames"] = "maybe"
        });

        Assert.Throws<InvalidOperationException>(() => MongodbTlsOptions.FromConfiguration(configuration));
    }

    [TestMethod]
    public void Merge_OverridesOnlyProvidedValues()
    {
        var baseline = MongodbTlsOptions.Create(caFile: "/certs/ca.crt", allowInvalidHostnames: false);
        var overrides = MongodbTlsOptions.Create(allowInvalidHostnames: true);

        var merged = MongodbTlsOptions.Merge(baseline, overrides);

        Assert.IsNotNull(merged);
        Assert.AreEqual("/certs/ca.crt", merged.CaFile);
        Assert.IsTrue(merged.AllowInvalidHostnames);
    }

    [TestMethod]
    public void Merge_ReturnsTheOtherSide_WhenOneSideIsNull()
    {
        var baseline = MongodbTlsOptions.Create(caFile: "/certs/ca.crt");

        Assert.AreSame(baseline, MongodbTlsOptions.Merge(baseline, null));
        Assert.AreSame(baseline, MongodbTlsOptions.Merge(null, baseline));
        Assert.IsNull(MongodbTlsOptions.Merge(null, null));
    }

    [TestMethod]
    public void Create_ReturnsNull_WhenEverythingIsEmpty()
    {
        Assert.IsNull(MongodbTlsOptions.Create());
        Assert.IsNull(MongodbTlsOptions.Create("", "  "));
    }

    [TestMethod]
    public void CacheKey_IsStable_ForEquivalentSettings()
    {
        var first = MongodbTlsOptions.Create(caFile: "/certs/ca.crt", allowInvalidHostnames: true);
        var second = MongodbTlsOptions.Create(caFile: "/certs/ca.crt", allowInvalidHostnames: true);
        var different = MongodbTlsOptions.Create(caFile: "/certs/other.crt", allowInvalidHostnames: true);

        Assert.AreEqual(first.CacheKey, second.CacheKey);
        Assert.AreNotEqual(first.CacheKey, different.CacheKey);
    }
}
