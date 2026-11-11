using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AgileConfig.Server.Data.Mongodb;

/// <summary>
///     Validates the certificate presented by a MongoDB server.
///     <para>
///         When <see cref="MongodbTlsOptions.CaFile" /> or <see cref="MongodbTlsOptions.Ca" /> is configured the
///         chain is rebuilt against that private trust store, which is what makes a self signed / internal CA
///         work with the .NET driver. Certificates that the operating system already trusts are still accepted,
///         so a connection string can be switched between a private cluster and a public one without changes.
///     </para>
/// </summary>
internal sealed class MongodbServerCertificateValidator
{
    private static readonly string[] PemMarkers = ["-----BEGIN CERTIFICATE-----", "-----BEGIN TRUSTED CERTIFICATE-----"];

    private readonly MongodbTlsOptions _options;
    private readonly Lazy<X509Certificate2Collection> _trustedRoots;

    private MongodbServerCertificateValidator(MongodbTlsOptions options)
    {
        _options = options;
        _trustedRoots = new Lazy<X509Certificate2Collection>(() => LoadTrustedRoots(options));
    }

    private X509Certificate2Collection TrustedRoots => _trustedRoots.Value;

    /// <summary>
    ///     Creates the callback assigned to <c>SslSettings.ServerCertificateValidationCallback</c>.
    /// </summary>
    public static RemoteCertificateValidationCallback Create(MongodbTlsOptions options)
    {
        var validator = new MongodbServerCertificateValidator(options);
        return validator.Validate;
    }

    private bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // The platform trust store already accepted the certificate; nothing else to check.
        if (sslPolicyErrors == SslPolicyErrors.None) return true;

        if (_options.AllowInvalidCertificates) return true;

        if (certificate is null) return false;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0) return false;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
        {
            var roots = TrustedRoots;
            if (roots.Count == 0) return false;
            if (!BuildChainWithPrivateTrustStore(certificate, chain, roots)) return false;
        }

        return _options.AllowInvalidHostnames ||
               (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == 0;
    }

    private bool BuildChainWithPrivateTrustStore(X509Certificate certificate, X509Chain driverChain,
        X509Certificate2Collection roots)
    {
        var leaf = certificate as X509Certificate2;
        var ownsLeaf = false;
        if (leaf is null)
        {
            leaf = X509CertificateLoader.LoadCertificate(certificate.GetRawCertData());
            ownsLeaf = true;
        }

        try
        {
            using var chainToValidate = new X509Chain();
            var policy = chainToValidate.ChainPolicy;
            policy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            policy.CustomTrustStore.AddRange(roots);
            policy.RevocationMode = _options.CheckCertificateRevocation
                ? X509RevocationMode.Online
                : X509RevocationMode.NoCheck;
            policy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            policy.VerificationFlags = X509VerificationFlags.NoFlag;

            // The driver already collected the intermediates sent by the server; feed them to our chain.
            if (driverChain is not null)
                foreach (var element in driverChain.ChainElements)
                    policy.ExtraStore.Add(element.Certificate);

            return chainToValidate.Build(leaf);
        }
        finally
        {
            if (ownsLeaf) leaf.Dispose();
        }
    }

    private static X509Certificate2Collection LoadTrustedRoots(MongodbTlsOptions options)
    {
        if (!string.IsNullOrEmpty(options.Ca)) return Parse(options.Ca, "db:tls:ca");

        if (string.IsNullOrEmpty(options.CaFile)) return [];

        var path = options.CaFile;
        if (!Path.IsPathRooted(path)) path = Path.Combine(AppContext.BaseDirectory, path);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"The MongoDB CA file configured by 'db:tls:caFile' was not found. " +
                $"Resolved path: '{path}'. When AgileConfig runs in a container, make sure the certificate is " +
                "mounted into the container, for example: -v /host/path/ca.crt:/certs/ca.crt:ro",
                path);

        var content = File.ReadAllBytes(path);
        if (LooksLikePem(content)) return Parse(Encoding.UTF8.GetString(content), path);

        // DER encoded certificate file.
        var collection = new X509Certificate2Collection();
        collection.Add(X509CertificateLoader.LoadCertificate(content));
        return collection;
    }

    private static X509Certificate2Collection Parse(string content, string source)
    {
        // Environment variables can not contain real line breaks, so "\n" is accepted as an escape.
        var text = content.Replace("\\r\\n", "\n").Replace("\\n", "\n").Trim();
        var collection = new X509Certificate2Collection();

        if (PemMarkers.Any(marker => text.Contains(marker, StringComparison.Ordinal)))
        {
            try
            {
                collection.ImportFromPem(text);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException(
                    $"Failed to read the MongoDB CA certificate(s) from '{source}'. The content looks like PEM " +
                    "but could not be parsed.", ex);
            }

            if (collection.Count == 0)
                throw new InvalidOperationException(
                    $"No certificate was found in the MongoDB CA configured by '{source}'.");

            return collection;
        }

        // A single line Base64 encoded DER certificate, the easiest form for an environment variable.
        try
        {
            collection.Add(X509CertificateLoader.LoadCertificate(Convert.FromBase64String(text)));
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Failed to read the MongoDB CA certificate(s) from '{source}'. Provide PEM text " +
                "(-----BEGIN CERTIFICATE-----) or a Base64 encoded DER certificate.", ex);
        }

        return collection;
    }

    private static bool LooksLikePem(byte[] content)
    {
        var text = Encoding.UTF8.GetString(content);
        return PemMarkers.Any(marker => text.Contains(marker, StringComparison.Ordinal));
    }
}
