using System;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Apisite;

public class Appsettings
{
    public const int DefaultPort = 5000;

    /// <summary>
    ///     Indicates whether the server runs in demo mode.
    /// </summary>
    public static bool IsPreviewMode =>
        "true".Equals(Global.Config["preview_mode"], StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     Indicates whether the admin console mode is enabled.
    /// </summary>
    public static bool IsAdminConsoleMode =>
        "true".Equals(Global.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     Indicates whether the cluster auto-join feature is enabled.
    /// </summary>
    public static bool Cluster => "true".Equals(Global.Config["cluster"], StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     The console port for admin UI. Defaults to 5000.
    /// </summary>
    public static int ConsolePort
    {
        get
        {
            if (int.TryParse(Global.Config?["consolePort"], out var port) && port > 0)
            {
                return port;
            }
            return DefaultPort;
        }
    }

    /// <summary>
    ///     The API port for client config pulling. Defaults to 5000.
    /// </summary>
    public static int ApiPort
    {
        get
        {
            if (int.TryParse(Global.Config?["apiPort"], out var port) && port > 0)
            {
                return port;
            }
            return DefaultPort;
        }
    }

    /// <summary>
    ///     path base
    /// </summary>
    public static string PathBase => Global.Config["pathBase"];

    /// <summary>
    ///     Indicates whether single sign-on is enabled.
    /// </summary>
    public static bool SsoEnabled =>
        "true".Equals(Global.Config["SSO:enabled"], StringComparison.CurrentCultureIgnoreCase);

    /// <summary>
    ///     SSO button text
    /// </summary>
    public static string SsoButtonText => Global.Config["SSO:loginButtonText"];

    public static string OtlpLogsEndpoint => Global.Config["otlp:logs:endpoint"];

    public static string OtlpLogsHeaders => Global.Config["otlp:logs:headers"];

    public static string OtlpLogsProtocol => Global.Config["otlp:logs:protocol"];

    public static string OtlpTracesEndpoint => Global.Config["otlp:traces:endpoint"];

    public static string OtlpTracesProtocol => Global.Config["otlp:traces:protocol"];

    public static string OtlpTracesHeaders => Global.Config["otlp:traces:headers"];

    public static string OtlpMetricsEndpoint => Global.Config["otlp:metrics:endpoint"];

    public static string OtlpMetricsProtocol => Global.Config["otlp:metrics:protocol"];

    public static string OtlpMetricsHeaders => Global.Config["otlp:metrics:headers"];

    public static string OtlpInstanceId => Global.Config["otlp:instanceId"];
}