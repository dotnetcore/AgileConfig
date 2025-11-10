using System;
using System.Reflection;
using AgileConfig.Server.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace AgileConfig.Server.Apisite;

public class Program
{
    public const string AppName = "AgileConfig Server";

    public static void Main(string[] args)
    {
        PrintBasicSysInfo();

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
#if DEBUG
        Global.Config =
            builder
                .AddJsonFile("appsettings.Development.json")
                .AddEnvironmentVariables()
                .Build();
#else
            Global.Config = builder.AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
#endif

        var host = CreateWebHostBuilder(args)
            .Build();

        host.Run();
    }

    private static void PrintBasicSysInfo()
    {
        var appVer = Assembly.GetAssembly(typeof(Program))?.GetName()?.Version?.ToString();
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        Console.WriteLine(ASCII_FONT.Font.Render("Agile Config"));
        Console.WriteLine("Version: {0}", appVer);
        Console.WriteLine("Path: {0}", basePath);
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        return WebHost.CreateDefaultBuilder(args).ConfigureLogging((context, builder) => { AddOtlpLogging(builder); }
            )
            .UseConfiguration(Global.Config)
            .UseStartup<Startup>();
    }

    private static void AddOtlpLogging(ILoggingBuilder builder)
    {
        if (string.IsNullOrEmpty(Appsettings.OtlpLogsEndpoint)) return;

        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(AppName
                , null, null, string.IsNullOrEmpty(Appsettings.OtlpInstanceId), Appsettings.OtlpInstanceId)
            );
            options
                .AddOtlpExporter(expOp =>
                {
                    expOp.Protocol = Appsettings.OtlpLogsProtocol == "http"
                        ? OtlpExportProtocol.HttpProtobuf
                        : OtlpExportProtocol.Grpc;
                    expOp.Endpoint = new Uri(Appsettings.OtlpLogsEndpoint);
                    if (!string.IsNullOrEmpty(Appsettings.OtlpLogsHeaders)) expOp.Headers = Appsettings.OtlpLogsHeaders;
                });
        });
    }
}