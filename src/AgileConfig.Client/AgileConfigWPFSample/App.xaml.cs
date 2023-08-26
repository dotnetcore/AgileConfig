using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AgileConfig.Client;

namespace AgileConfigWPFSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ConfigClient ConfigClient { get; private set; }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //跟控制台项目一样，appid等信息取决于你如何获取。你可以写死，可以从配置文件读取，可以从别的web service读取。
            var appId = "test_app";
            var secret = "test_app";
            var nodes = "http://agileconfig_server.xbaby.xyz/";
            ConfigClient = new ConfigClient(appId, secret, nodes, "DEV");
            ConfigClient.Name = "wpfconfigclient";
            ConfigClient.Tag = "t1";
            ConfigClient.ConnectAsync().GetAwaiter();
        }
    }
}
