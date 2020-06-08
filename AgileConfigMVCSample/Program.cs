using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Agile.Config.Client;
using Agile.Config.Protocol;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                //读取本地配置
                var localconfig = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json").Build();
                //从本地配置里读取AgileConfig的相关信息
                var appId = localconfig["AgileConfig:appId"];
                var secret = localconfig["AgileConfig:secret"];
                var nodes = localconfig["AgileConfig:nodes"];
                //new一个client实例
                var configClient = new ConfigClient(appId, secret, nodes);
                //使用AddAgileConfig配置一个新的IConfigurationSource
                config.AddAgileConfig(configClient);
                //找一个变量挂载client实例，以便其他地方可以直接使用实例访问配置
                ConfigClient = configClient;
                //注册配置项修改事件
                configClient.ConfigChanged += ConfigClient_ConfigChanged;
            })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// 此事件会在配置项目发生新增、修改、删除的时候触发
        /// </summary>
        private static void ConfigClient_ConfigChanged(ConfigChangedArg obj)
        {
            Console.WriteLine($"action:{obj.Action} key:{obj.Key}");

            switch (obj.Action)
            {
                case ActionConst.Add:
                    break;
                case ActionConst.Update:
                    break;
                case ActionConst.Remove:
                    break;
                default:
                    break;
            }
        }
    }
}
