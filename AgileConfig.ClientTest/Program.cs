using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
namespace AgileConfig.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var lf = serviceProvider.GetService<ILoggerFactory>();

            var appId = "xxx";
            var seret = "";
            var host = "http://localhost:5000";

            try
            {
                var provider = new AgileConfig.Client.Configuration.AgileConfigProvider(host, appId, seret, lf);
                provider.Load();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddConsole());
            services.Configure<LoggerFilterOptions>(op => {
                op.MinLevel = LogLevel.Trace;
            });
        }
    }
}
