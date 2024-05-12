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

        public static void AddOtlp(this IServiceCollection services)
        {
            //services.AddOpenTelemetry()
            //          .ConfigureResource(resource => resource.AddService(Program.AppName))
            //          .WithTracing(tracing => tracing
            //          .AddAspNetCoreInstrumentation()
            //          .AddHttpClientInstrumentation() 
            //                                    .AddOtlpExporter(op =>
            //                                    {
            //                                        op.Protocol = OtlpExportProtocol.HttpProtobuf;
            //                                        op.Endpoint = new System.Uri(Global.Config["otlp:traces:endpoint"]);
            //                                    })
            //                      )
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
