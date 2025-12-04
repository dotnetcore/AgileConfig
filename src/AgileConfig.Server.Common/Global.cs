using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Common;

public static class Global
{
    public const string DefaultHttpClientName = "Default";
    public static IConfiguration Config { get; set; }

    public static ILoggerFactory LoggerFactory { get; set; }
}