using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using AgileConfig.Server.Data.Mongodb;

namespace AgileConfig.Server.Data.MongodbTests;

[TestClass]
public class MongodbServerCertificateValidatorTests
{
    private const string NodeHostName = "mdb1.dpangzi.com";

    private static (X509Certificate2 Ca, X509Certificate2 Leaf) CreateCaAndLeaf(string dnsName)
    {
        using var caKey = RSA.Create(2048);
        var caRequest = new CertificateRequest("CN=AgileConfig Test Root CA", caKey,
            HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        caRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
        caRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(caRequest.PublicKey, false));
        var ca = caRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        using var leafKey = RSA.Create(2048);
        var leafRequest = new CertificateRequest($"CN={dnsName}", leafKey, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        leafRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
        leafRequest.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, false));
        var subjectAlternativeName = new SubjectAlternativeNameBuilder();
        subjectAlternativeName.AddDnsName(dnsName);
        leafRequest.CertificateExtensions.Add(subjectAlternativeName.Build());

        var serialNumber = new byte[8];
        RandomNumberGenerator.Fill(serialNumber);
        var leaf = leafRequest.Create(ca, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddMonths(6),
            serialNumber);

        return (ca, leaf);
    }

    [TestMethod]
    public void Accepts_CertificateSignedByTheConfiguredCa()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: ca.ExportCertificatePem()));

        Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [TestMethod]
    public void Rejects_CertificateSignedByAnotherCa()
    {
        var (_, leaf) = CreateCaAndLeaf(NodeHostName);
        var (otherCa, _) = CreateCaAndLeaf(NodeHostName);
        var callback =
            MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: otherCa.ExportCertificatePem()));

        Assert.IsFalse(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [TestMethod]
    public void Rejects_HostNameMismatch()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: ca.ExportCertificatePem()));

        var errors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch;

        Assert.IsFalse(callback(null, leaf, null, errors));
    }

    [TestMethod]
    public void Accepts_HostNameMismatch_WhenAllowInvalidHostnamesIsEnabled()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(
            MongodbTlsOptions.Create(ca: ca.ExportCertificatePem(), allowInvalidHostnames: true));

        var errors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch;

        Assert.IsTrue(callback(null, leaf, null, errors));
    }

    [TestMethod]
    public void Accepts_Anything_WhenThePlatformAlreadyTrustsTheCertificate()
    {
        var (_, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(caFile: "/does/not/matter"));

        Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.None));
    }

    [TestMethod]
    public void Accepts_Anything_WhenAllowInvalidCertificatesIsEnabled()
    {
        var (_, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(
            MongodbTlsOptions.Create(allowInvalidCertificates: true));

        Assert.IsTrue(callback(null, leaf, null,
            SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch));
    }

    [TestMethod]
    public void Rejects_ChainErrors_WhenNoCaIsConfigured()
    {
        var (_, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(
            MongodbTlsOptions.Create(allowInvalidHostnames: true));

        Assert.IsFalse(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [TestMethod]
    public void Rejects_WhenNoCertificateWasPresented()
    {
        var (ca, _) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: ca.ExportCertificatePem()));

        Assert.IsFalse(callback(null, null, null, SslPolicyErrors.RemoteCertificateNotAvailable));
    }

    [TestMethod]
    public void Accepts_InlinePem_WithEscapedNewLines()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var escaped = ca.ExportCertificatePem().Replace("\n", "\\n");

        // This is the form that fits into a docker environment variable.
        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: escaped));

        Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }

    [TestMethod]
    public void Accepts_CaFile_OnDisk()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var path = Path.Combine(Path.GetTempPath(), $"agileconfig-ca-{Guid.NewGuid():N}.crt");

        try
        {
            File.WriteAllText(path, ca.ExportCertificatePem());

            var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(caFile: path));

            Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Accepts_CaFile_WithRelativePath()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var fileName = $"agileconfig-ca-{Guid.NewGuid():N}.crt";
        var path = Path.Combine(AppContext.BaseDirectory, fileName);

        try
        {
            File.WriteAllText(path, ca.ExportCertificatePem());

            var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(caFile: fileName));

            Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void Throws_WithMountHint_WhenCaFileDoesNotExist()
    {
        var (_, leaf) = CreateCaAndLeaf(NodeHostName);
        var callback = MongodbServerCertificateValidator.Create(
            MongodbTlsOptions.Create(caFile: "/certs/missing-ca.crt"));

        var exception = Assert.Throws<FileNotFoundException>(() =>
            callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));

        StringAssert.Contains(exception.Message, "mounted");
    }

    [TestMethod]
    public void Accepts_Base64EncodedDerCa()
    {
        var (ca, leaf) = CreateCaAndLeaf(NodeHostName);
        var base64 = Convert.ToBase64String(ca.Export(X509ContentType.Cert));

        var callback = MongodbServerCertificateValidator.Create(MongodbTlsOptions.Create(ca: base64));

        Assert.IsTrue(callback(null, leaf, null, SslPolicyErrors.RemoteCertificateChainErrors));
    }
}
