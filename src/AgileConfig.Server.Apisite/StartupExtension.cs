using AgileConfig.Server.Common;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Net.Http;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;
using OpenTelemetry;

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
                          .AddHttpClientInstrumentation()
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
                          .AddOtlpExporter(op =>
                              {
                                  op.Protocol = Appsettings.OtlpMetricsProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                                  op.Endpoint = new System.Uri(Appsettings.OtlpMetricsEndpoint);
                              })
                      );

            return builder;
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
