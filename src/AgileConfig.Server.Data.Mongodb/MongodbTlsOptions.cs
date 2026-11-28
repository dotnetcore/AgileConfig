using System.Text;
using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.Mongodb;

/// <summary>
///     TLS settings used when the MongoDB server presents a certificate that is not signed by a
///     publicly trusted CA (for example a private CA deployed together with a replica set).
///     <para>
///         Everything is optional. When no setting is provided the MongoDB driver keeps its default
///         behavior, which means the current deployments are not affected at all.
///     </para>
///     <para>
///         Both <c>db:tls:*</c> and the per environment <c>db:env:{env}:tls:*</c> configuration keys are
///         supported, as well as the <c>tlsCAFile</c> family of connection string options that other
///         MongoDB drivers accept but the .NET driver does not.
///     </para>
/// </summary>
public sealed class MongodbTlsOptions
{
    /// <summary>
    ///     Configuration section (relative to <c>db:</c> or <c>db:env:{env}:</c>) that holds the settings.
    /// </summary>
    public const string ConfigSection = "tls";

    private readonly Raw _raw;

    private MongodbTlsOptions(Raw raw)
    {
        _raw = raw;
    }

    /// <summary>
    ///     Path of a PEM or DER file that contains one or more trusted root (CA) certificates.
    ///     A relative path is resolved against the application base directory.
    /// </summary>
    public string CaFile => _raw.CaFile;

    /// <summary>
    ///     Inline trusted root (CA) certificate(s). Accepts PEM text, a PEM text whose line breaks were
    ///     escaped as <c>\n</c> (handy for environment variables), or Base64 encoded DER.
    /// </summary>
    public string Ca => _raw.Ca;

    /// <summary>
    ///     Skip the certificate chain validation completely. Meant for local debugging only.
    /// </summary>
    public bool AllowInvalidCertificates => _raw.AllowInvalidCertificates ?? false;

    /// <summary>
    ///     Skip the host name check while keeping certificate chain validation.
    /// </summary>
    public bool AllowInvalidHostnames => _raw.AllowInvalidHostnames ?? false;

    /// <summary>
    ///     Check certificate revocation for chains built from <see cref="CaFile" /> / <see cref="Ca" />.
    /// </summary>
    public bool CheckCertificateRevocation => _raw.CheckCertificateRevocation ?? false;

    /// <summary>
    ///     True when the instance carries no effective setting, i.e. when it can not change the
    ///     behavior of the MongoDB driver. An explicit <c>false</c> is the default value of every
    ///     boolean option and does not require the custom TLS branch on its own.
    /// </summary>
    public bool IsEmpty => _raw.IsEmpty;

    /// <summary>
    ///     Reads the TLS settings from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="env">Optional environment name; <c>db:env:{env}:tls:*</c> overrides <c>db:tls:*</c> per key.</param>
    /// <returns>
    ///     The settings, or <c>null</c> when no effective setting is configured. Configuration keys that
    ///     merely repeat a default value (for example <c>allowInvalidHostnames=false</c> in
    ///     appsettings.json) do not count as an effective setting.
    /// </returns>
    public static MongodbTlsOptions FromConfiguration(IConfiguration configuration, string env = null)
    {
        if (configuration is null) return null;

        var raw = Read(configuration, $"db:{ConfigSection}");
        if (!string.IsNullOrWhiteSpace(env)) raw = raw.Override(Read(configuration, $"db:env:{env}:{ConfigSection}"));

        return raw.IsEmpty ? null : new MongodbTlsOptions(raw);
    }

    /// <summary>
    ///     Merges two settings instances. Values present in <paramref name="overrides" /> win, values that
    ///     are missing there fall back to <paramref name="baseline" />.
    /// </summary>
    public static MongodbTlsOptions Merge(MongodbTlsOptions baseline, MongodbTlsOptions overrides)
    {
        if (baseline is null) return overrides;
        if (overrides is null) return baseline;

        var raw = baseline._raw.Override(overrides._raw);
        return raw.IsEmpty ? null : new MongodbTlsOptions(raw);
    }

    /// <summary>
    ///     Creates an instance from explicit values. Used by tests and by the connection string parser.
    ///     <para>
    ///         An explicit <c>false</c> is kept so that it can override a <c>true</c> value coming from a
    ///         lower precedence source, but the resulting instance is still <see cref="IsEmpty" /> because
    ///         it does not enable any custom TLS behavior.
    ///     </para>
    /// </summary>
    public static MongodbTlsOptions Create(
        string caFile = null,
        string ca = null,
        bool? allowInvalidCertificates = null,
        bool? allowInvalidHostnames = null,
        bool? checkCertificateRevocation = null)
    {
        var raw = new Raw(Trim(caFile), Trim(ca), allowInvalidCertificates, allowInvalidHostnames,
            checkCertificateRevocation);
        return raw.HasAnyValue ? new MongodbTlsOptions(raw) : null;
    }

    /// <summary>
    ///     Key used to cache one MongoDB client per connection string + TLS configuration combination.
    /// </summary>
    internal string CacheKey
    {
        get
        {
            var caHash = string.IsNullOrEmpty(_raw.Ca)
                ? ""
                : Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(_raw.Ca)));
            return string.Join('|', _raw.CaFile ?? "", caHash,
                AllowInvalidCertificates, AllowInvalidHostnames, CheckCertificateRevocation);
        }
    }

    private static Raw Read(IConfiguration configuration, string section)
    {
        var caFile = Trim(configuration[$"{section}:caFile"]);
        var ca = Trim(configuration[$"{section}:ca"]);
        var allowInvalidCertificates = ReadBool(configuration[$"{section}:allowInvalidCertificates"]);
        var allowInvalidHostnames = ReadBool(configuration[$"{section}:allowInvalidHostnames"]);
        var checkCertificateRevocation = ReadBool(configuration[$"{section}:checkCertificateRevocation"]);

        return new Raw(caFile, ca, allowInvalidCertificates, allowInvalidHostnames, checkCertificateRevocation);
    }

    private static string Trim(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool? ReadBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();
        if (bool.TryParse(trimmed, out var parsed)) return parsed;
        // Common environment variable spellings.
        if (trimmed is "1" or "yes" or "on") return true;
        if (trimmed is "0" or "no" or "off") return false;

        throw new InvalidOperationException(
            $"MongoDB TLS configuration value '{value}' is not a valid boolean. Use true or false.");
    }

    /// <summary>
    ///     All explicitly provided values. Keeping them nullable lets an environment specific setting
    ///     override a single key of the global setting without resetting the others.
    /// </summary>
    private readonly record struct Raw(
        string CaFile,
        string Ca,
        bool? AllowInvalidCertificates,
        bool? AllowInvalidHostnames,
        bool? CheckCertificateRevocation)
    {
        /// <summary>
        ///     True when at least one key was provided, even if its value is the default (for example
        ///     an explicit <c>false</c>). Such a value still has to take part in precedence handling.
        /// </summary>
        public bool HasAnyValue => CaFile is not null || Ca is not null ||
                                   AllowInvalidCertificates is not null || AllowInvalidHostnames is not null ||
                                   CheckCertificateRevocation is not null;

        /// <summary>
        ///     True when no setting can change the driver behavior: there is no CA to trust and no
        ///     option is enabled. Explicit <c>false</c> values are deliberately ignored here.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(CaFile) && string.IsNullOrWhiteSpace(Ca) &&
                               AllowInvalidCertificates is not true && AllowInvalidHostnames is not true &&
                               CheckCertificateRevocation is not true;

        public Raw Override(Raw overrides)
        {
            return new Raw(
                overrides.CaFile ?? CaFile,
                overrides.Ca ?? Ca,
                overrides.AllowInvalidCertificates ?? AllowInvalidCertificates,
                overrides.AllowInvalidHostnames ?? AllowInvalidHostnames,
                overrides.CheckCertificateRevocation ?? CheckCertificateRevocation);
        }
    }
}
