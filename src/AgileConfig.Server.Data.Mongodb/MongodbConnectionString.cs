namespace AgileConfig.Server.Data.Mongodb;

/// <summary>
///     Normalizes a MongoDB connection string before it is handed to the .NET driver.
///     <para>
///         The .NET driver rejects the <c>tlsCAFile</c> / <c>tlsAllowInvalidCertificates</c> /
///         <c>tlsAllowInvalidHostnames</c> options that the MongoDB shell and most other official drivers
///         accept. AgileConfig understands those options itself: they are removed from the connection
///         string and translated into a <see cref="MongodbTlsOptions" /> instead, so the same connection
///         string can be reused across drivers.
///     </para>
/// </summary>
internal static class MongodbConnectionString
{
    private const string TlsCaFileKey = "tlscafile";
    private const string SslCaFileKey = "sslcafile";
    private const string TlsAllowInvalidCertificatesKey = "tlsallowinvalidcertificates";
    private const string SslAllowInvalidCertificatesKey = "sslallowinvalidcertificates";
    private const string TlsAllowInvalidHostnamesKey = "tlsallowinvalidhostnames";
    private const string SslAllowInvalidHostnamesKey = "sslallowinvalidhostnames";
    private const string TlsCertificateKeyFileKey = "tlscertificatekeyfile";
    private const string SslCertificateKeyFileKey = "sslcertificatekeyfile";

    /// <summary>
    ///     Removes the TLS options the .NET driver does not understand and returns them separately.
    /// </summary>
    /// <returns>The connection string to pass to the driver, plus the extracted TLS options (or null).</returns>
    public static NormalizedConnectionString Normalize(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return new NormalizedConnectionString(connectionString, null);

        var queryIndex = connectionString.IndexOf('?');
        if (queryIndex < 0) return new NormalizedConnectionString(connectionString, null);

        var prefix = connectionString[..queryIndex];
        var query = connectionString[(queryIndex + 1)..];

        var kept = new List<string>();
        string caFile = null;
        bool? allowInvalidCertificates = null;
        bool? allowInvalidHostnames = null;
        var optionsFound = false;

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = pair.IndexOf('=');
            var key = (separatorIndex < 0 ? pair : pair[..separatorIndex]).Trim().ToLowerInvariant();
            var value = separatorIndex < 0 ? "" : pair[(separatorIndex + 1)..];

            switch (key)
            {
                case TlsCaFileKey:
                case SslCaFileKey:
                    caFile = Decode(value);
                    optionsFound = true;
                    break;
                case TlsAllowInvalidCertificatesKey:
                case SslAllowInvalidCertificatesKey:
                    allowInvalidCertificates = ParseBool(key, value);
                    optionsFound = true;
                    break;
                case TlsAllowInvalidHostnamesKey:
                case SslAllowInvalidHostnamesKey:
                    allowInvalidHostnames = ParseBool(key, value);
                    optionsFound = true;
                    break;
                case TlsCertificateKeyFileKey:
                case SslCertificateKeyFileKey:
                    throw new NotSupportedException(
                        $"The MongoDB connection string option '{key}' (client certificate / mTLS) is not supported by AgileConfig yet. " +
                        "Remove it and use a server certificate issued by a CA that can be configured with 'db:tls:caFile' instead.");
                default:
                    kept.Add(pair);
                    break;
            }
        }

        if (!optionsFound) return new NormalizedConnectionString(connectionString, null);

        var normalized = kept.Count == 0 ? prefix : $"{prefix}?{string.Join('&', kept)}";
        var tlsOptions = MongodbTlsOptions.Create(caFile, null, allowInvalidCertificates, allowInvalidHostnames);

        return new NormalizedConnectionString(normalized, tlsOptions);
    }

    private static string Decode(string value)
    {
        return Uri.UnescapeDataString(value.Replace("+", "%20"));
    }

    private static bool ParseBool(string key, string value)
    {
        var decoded = Decode(value).Trim();
        if (bool.TryParse(decoded, out var parsed)) return parsed;

        throw new InvalidOperationException(
            $"The MongoDB connection string option '{key}' must be 'true' or 'false', but its value is '{value}'.");
    }
}

/// <summary>
///     Result of <see cref="MongodbConnectionString.Normalize" />.
/// </summary>
internal readonly record struct NormalizedConnectionString(string ConnectionString, MongodbTlsOptions TlsOptions);
