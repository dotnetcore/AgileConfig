﻿using System;
using AgileConfig.Server.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

namespace AgileConfig.Server.Apisite
{
    public class Program
    {
        public const string AppName = "AgileConfig Server";

        public static void Main(string[] args)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("current dir path: " + basePath);
            var builder = new ConfigurationBuilder()
            .SetBasePath(basePath);
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

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).ConfigureLogging(
                    (context, builder) =>
                    {
                        AddOtlpLogging(builder);
                    }
                  )
                  .UseConfiguration(Global.Config)
                  .UseStartup<Startup>();
        }

        private static void AddOtlpLogging(ILoggingBuilder builder)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpLogsEndpoint))
            {
                return;
            }

            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Program.AppName
                    , null, null, string.IsNullOrEmpty(Appsettings.OtlpInstanceId), Appsettings.OtlpInstanceId)
                    );
                options
                       .AddOtlpExporter(expOp =>
                       {
                           expOp.Protocol = Appsettings.OtlpLogsProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                           expOp.Endpoint = new Uri(Appsettings.OtlpLogsEndpoint);
                           if (!string.IsNullOrEmpty(Appsettings.OtlpLogsHeaders))
                           {
                               expOp.Headers = Appsettings.OtlpLogsHeaders;    
                           }
                       });
            });
        }

    }
}
