using AgileConfig.Server.Data.Mongodb;
using MongoDB.Driver;

namespace AgileConfig.Server.Data.MongodbTests;

[TestClass]
public class MongodbAccessTests
{
    private const string PlainConnectionString = "mongodb://localhost:27017/AgileConfig";

    private static MongoClientSettings BuildSettings(string connectionString, MongodbTlsOptions tlsOptions)
    {
        var normalized = MongodbConnectionString.Normalize(connectionString);
        var effectiveTlsOptions = MongodbTlsOptions.Merge(tlsOptions, normalized.TlsOptions);

        return MongodbAccess.CreateClientSettings(MongoUrl.Create(normalized.ConnectionString),
            normalized.ConnectionString, effectiveTlsOptions);
    }

    [TestMethod]
    public void DefaultAppSettings_KeepTlsOff_ForAPlainConnectionString()
    {
        var tlsOptions = MongodbTlsOptions.FromConfiguration(TestConfiguration.LoadDefaultAppSettings());
        Assert.IsNull(tlsOptions);

        var settings = BuildSettings(PlainConnectionString, tlsOptions);

        Assert.IsFalse(settings.UseTls);
        Assert.IsNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }

    [TestMethod]
    public void DefaultAppSettings_DoNotOverrideExplicitTlsFalse()
    {
        var tlsOptions = MongodbTlsOptions.FromConfiguration(TestConfiguration.LoadDefaultAppSettings());

        var settings = BuildSettings($"{PlainConnectionString}?tls=false", tlsOptions);

        Assert.IsFalse(settings.UseTls);
        Assert.IsNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }

    [TestMethod]
    public void ExplicitTlsFalse_IsRespected_EvenWhenACaIsConfigured()
    {
        var tlsOptions = MongodbTlsOptions.Create(ca: "not-used-while-tls-is-disabled");

        var settings = BuildSettings($"{PlainConnectionString}?tls=false", tlsOptions);

        Assert.IsFalse(settings.UseTls);
        Assert.IsNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }

    [TestMethod]
    public void ExplicitLegacySslFalse_IsRespected_EvenWhenACaIsConfigured()
    {
        var tlsOptions = MongodbTlsOptions.Create(ca: "not-used-while-tls-is-disabled");

        var settings = BuildSettings($"{PlainConnectionString}?ssl=false", tlsOptions);

        Assert.IsFalse(settings.UseTls);
    }

    [TestMethod]
    public void ConfiguredCa_TurnsTlsOn_WhenTheConnectionStringDoesNotMentionIt()
    {
        var tlsOptions = MongodbTlsOptions.Create(ca: "not-loaded-until-the-callback-runs");

        var settings = BuildSettings(PlainConnectionString, tlsOptions);

        Assert.IsTrue(settings.UseTls);
        Assert.IsNotNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }

    [TestMethod]
    public void ExplicitTlsTrue_KeepsTlsOn_AndInstallsTheCustomValidator()
    {
        var tlsOptions = MongodbTlsOptions.Create(ca: "not-loaded-until-the-callback-runs");

        var settings = BuildSettings($"{PlainConnectionString}?tls=true", tlsOptions);

        Assert.IsTrue(settings.UseTls);
        Assert.IsNotNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }

    [TestMethod]
    public void DefaultOnlyFalseOptions_DoNotInstallTheCustomValidator()
    {
        // An explicit false can arrive from an environment override even when FromConfiguration
        // already collapsed the default configuration to null.
        var tlsOptions = MongodbTlsOptions.Create(allowInvalidHostnames: false, checkCertificateRevocation: false);
        Assert.IsTrue(tlsOptions.IsEmpty);

        var settings = BuildSettings(PlainConnectionString, tlsOptions);

        Assert.IsFalse(settings.UseTls);
        Assert.IsNull(settings.SslSettings?.ServerCertificateValidationCallback);
    }
}
