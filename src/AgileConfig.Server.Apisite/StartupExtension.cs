using AgileConfig.Server.Common;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.Net.Http;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

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

        public static void AddOtlpTraces(this IServiceCollection services)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpTracesEndpoint))
            {
                return;
            }

            services.AddOpenTelemetry()
                      .ConfigureResource(resource => resource.AddService(Program.AppName))
                      .WithTracing(tracing => tracing
                      .AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddOtlpExporter(op =>
                          {
                              op.Protocol = Appsettings.OtlpTracesProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                              op.Endpoint = new System.Uri(Appsettings.OtlpTracesEndpoint);
                          })
                       )
                      //.WithMetrics(metrics => metrics
                      //                        .AddRuntimeInstrumentation()
                      //                        .AddAspNetCoreInstrumentation()
                      //                        .AddOtlpExporter(op =>
                      //                        {
                      //                            op.Protocol = OtlpExportProtocol.HttpProtobuf;
                      //                            op.Endpoint = new System.Uri(Global.Config["otlp:trace:endpoint"]);
                      //                        })
                      //              )
                      ;
        }

        public static void AddOtlpMetrics(this IServiceCollection services)
        {
            if (string.IsNullOrEmpty(Appsettings.OtlpMetricsEndpoint))
            {
                return;
            }

            services.AddOpenTelemetry()
                      .ConfigureResource(resource => resource.AddService(Program.AppName))
                      .WithMetrics(metrics => metrics
                                              .AddRuntimeInstrumentation()
                                              .AddAspNetCoreInstrumentation()
                                              .AddOtlpExporter(op =>
                                              {
                                                  op.Protocol = Appsettings.OtlpMetricsProtocol == "http" ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
                                                  op.Endpoint = new System.Uri(Appsettings.OtlpMetricsEndpoint);
                                              })
                                    )
                      ;
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
