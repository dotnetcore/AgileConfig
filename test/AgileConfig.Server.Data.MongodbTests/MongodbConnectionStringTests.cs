using AgileConfig.Server.Data.Mongodb;

namespace AgileConfig.Server.Data.MongodbTests;

[TestClass]
public class MongodbConnectionStringTests
{
    [TestMethod]
    public void Normalize_ExtractsTlsCaFile()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://u:p@h1:27017,h2:27017/db?replicaSet=rs0&authSource=admin&tls=true&tlsCAFile=%2Fcerts%2Fca.crt");

        Assert.AreEqual("mongodb://u:p@h1:27017,h2:27017/db?replicaSet=rs0&authSource=admin&tls=true",
            normalized.ConnectionString);
        Assert.IsNotNull(normalized.TlsOptions);
        Assert.AreEqual("/certs/ca.crt", normalized.TlsOptions.CaFile);
    }

    [TestMethod]
    public void Normalize_AcceptsTheLegacySslPrefix()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://localhost:27017/db?ssl=true&sslCAFile=/tmp/ca.crt&sslAllowInvalidHostnames=true");

        Assert.AreEqual("mongodb://localhost:27017/db?ssl=true", normalized.ConnectionString);
        Assert.IsNotNull(normalized.TlsOptions);
        Assert.AreEqual("/tmp/ca.crt", normalized.TlsOptions.CaFile);
        Assert.IsTrue(normalized.TlsOptions.AllowInvalidHostnames);
    }

    [TestMethod]
    public void Normalize_ExtractsAllowInvalidCertificates()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://localhost:27017/db?tls=true&tlsAllowInvalidCertificates=true");

        Assert.AreEqual("mongodb://localhost:27017/db?tls=true", normalized.ConnectionString);
        Assert.IsNotNull(normalized.TlsOptions);
        Assert.IsTrue(normalized.TlsOptions.AllowInvalidCertificates);
    }

    [TestMethod]
    public void Normalize_KeepsOptionsTheDriverUnderstands()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://localhost:27017/db?tls=true&tlsInsecure=true&tlsDisableCertificateRevocationCheck=true");

        Assert.AreEqual(
            "mongodb://localhost:27017/db?tls=true&tlsInsecure=true&tlsDisableCertificateRevocationCheck=true",
            normalized.ConnectionString);
        Assert.IsNull(normalized.TlsOptions);
    }

    [TestMethod]
    public void Normalize_ReturnsOriginalString_WhenThereIsNoQuery()
    {
        var normalized = MongodbConnectionString.Normalize("mongodb://localhost:27017/db");

        Assert.AreEqual("mongodb://localhost:27017/db", normalized.ConnectionString);
        Assert.IsNull(normalized.TlsOptions);
    }

    [TestMethod]
    public void Normalize_ReturnsOriginalString_WhenNoTlsOptionIsPresent()
    {
        const string connectionString = "mongodb://localhost:27017/db?replicaSet=rs0";
        var normalized = MongodbConnectionString.Normalize(connectionString);

        Assert.AreEqual(connectionString, normalized.ConnectionString);
        Assert.IsNull(normalized.TlsOptions);
    }

    [TestMethod]
    public void Normalize_RemovesTheQuestionMark_WhenEveryOptionIsExtracted()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://localhost:27017/db?tlsCAFile=/tmp/ca.crt");

        Assert.AreEqual("mongodb://localhost:27017/db", normalized.ConnectionString);
        Assert.IsNotNull(normalized.TlsOptions);
    }

    [TestMethod]
    public void Normalize_IsCaseInsensitive()
    {
        var normalized = MongodbConnectionString.Normalize(
            "mongodb://localhost:27017/db?TLS=true&TLSCAFILE=/tmp/ca.crt");

        Assert.AreEqual("mongodb://localhost:27017/db?TLS=true", normalized.ConnectionString);
        Assert.AreEqual("/tmp/ca.crt", normalized.TlsOptions.CaFile);
    }

    [TestMethod]
    public void Normalize_Throws_ForClientCertificateOption()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            MongodbConnectionString.Normalize(
                "mongodb://localhost:27017/db?tls=true&tlsCertificateKeyFile=/tmp/client.pem"));

        StringAssert.Contains(exception.Message, "mTLS");
    }

    [TestMethod]
    public void Normalize_Throws_ForInvalidBooleanValue()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MongodbConnectionString.Normalize(
                "mongodb://localhost:27017/db?tls=true&tlsAllowInvalidHostnames=maybe"));
    }

    [TestMethod]
    public void Normalize_HandlesNullAndEmptyInput()
    {
        Assert.IsNull(MongodbConnectionString.Normalize(null).ConnectionString);
        Assert.AreEqual(string.Empty, MongodbConnectionString.Normalize(string.Empty).ConnectionString);
    }
}
