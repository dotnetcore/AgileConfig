using AgileConfig.Server.Common;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using System.Net.Http;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using OpenTelemetry;
using Npgsql;
using AgileConfig.Server.Apisite.Metrics;

namespace AgileConfig.Server.Apisite
{
    public static class StartupExtension
    {
        public static void AddDefaultHttpClient(this IServiceCollection services, bool isTrustSSL)
        {
            services.AddHttpClient(Global.DefaultHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return NewMessageHandler(isTrustSSL);
                })
                ;
        }

        public static IOpenTelemetryBuilder AddOtlpTraces(this IOpenTelemetryBuilder builder)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpTracesEndpoint))
            {
                return builder;
            }

            builder.WithTracing(tracing => tracing
                          .AddAspNetCoreInstrumentation()
                          .AddNpgsql()
                          .AddOtlpExporter(op =>
                              {
                                  op.Protocol = Appsettings.OtlpTracesProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                                  op.Endpoint = new System.Uri(Appsettings.OtlpTracesEndpoint);
                              })
                       );

            return builder;
        }

        public static IOpenTelemetryBuilder AddOtlpMetrics(this IOpenTelemetryBuilder builder)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpMetricsEndpoint))
            {
                return builder;
            }

            builder.WithMetrics(metrics => metrics
                          .AddAspNetCoreInstrumentation()
                          .AddRuntimeInstrumentation()
                          .AddMeter(MeterService.MeterName)
                          .AddOtlpExporter((op, reader) =>
                              {
                                  op.Protocol = Appsettings.OtlpMetricsProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                                  op.Endpoint = new System.Uri(Appsettings.OtlpMetricsEndpoint);
                                  reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
                              })
                      );

            return builder;
        }

        public static IServiceCollection AddMeterService(this IServiceCollection services)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpMetricsEndpoint))
            {
                return services;
            }

            services.AddResourceMonitoring();
            services.AddSingleton<IMeterService, MeterService>();

            return services;
        }

        static HttpMessageHandler NewMessageHandler(bool alwaysTrustSsl)
        {
            var handler = new HttpClientHandler();
            if (alwaysTrustSsl)
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    return true;
                };
            }

            return handler;
        }
    }
}
