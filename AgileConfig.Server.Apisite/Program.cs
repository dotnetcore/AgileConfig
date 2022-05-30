using System;
using System.IO;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace AgileConfig.Server.Apisite
{
    public class Program
    {
        //public static IRemoteServerNodeProxy RemoteServerNodeProxy { get; private set; }

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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        {
            return WebHost.CreateDefaultBuilder(args)
                  .UseConfiguration(Global.Config)
                  .UseNLog()
                  .UseStartup<Startup>();
        }
          
    }
}
