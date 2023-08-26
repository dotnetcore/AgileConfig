using AgileConfig.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace AgileConfigMVCSample
{
    public class Program
    {
        public static IConfigClient ConfigClient;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                //new一个client实例
                //使用无参构造函数会自动读取本地appsettings.json文件的AgileConfig节点的配置
                var configClient = new ConfigClient();
               
                //注册配置项修改事件
                configClient.ConfigChanged += (arg) =>
                {
                    Console.WriteLine($"action:{arg.Action} key:{arg.Key}");
                };

                //使用AddAgileConfig配置一个新的IConfigurationSource
                config.AddAgileConfig(configClient);
            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
