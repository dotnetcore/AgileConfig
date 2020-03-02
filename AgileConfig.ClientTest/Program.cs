using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
            var seret = "app1";
            var host = "http://localhost:5000";

            try
            {
                var provider = new AgileConfig.Client.AgileConfigProvider(host, appId, seret, lf);
                provider.Load();
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(5000);
                        foreach (string key in Client.ConfigClient.Instance.Data.Keys)
                        {
                            var val = Client.ConfigClient.Instance[key];
                            Console.WriteLine("{0} : {1}", key, val);
                        }
                    }
                });
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
            services.Configure<LoggerFilterOptions>(op =>
            {
                op.MinLevel = LogLevel.Trace;
            });
        }
    }
}
