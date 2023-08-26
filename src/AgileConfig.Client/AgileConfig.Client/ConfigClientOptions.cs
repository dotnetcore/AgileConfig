using AgileConfig.Client.RegisterCenter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AgileConfig.Client
{
    public class ConfigClientOptions
    {
        public string AppId { get; set; }

        public string Secret { get; set; }

        public string Nodes { get; set; }

        public string Name { get; set; }

        public string Tag { get; set; }

        public string ENV { get; set; }

        public int HttpTimeout { get; set; } = 100;

        public string CacheDirectory { get; set; }

        public ServiceRegisterInfo RegisterInfo { get; set; }

        public ILogger Logger { get; set; }

        public Action<ConfigChangedArg> ConfigChanged;

        /// <summary>
        /// 确定当前目录是否存在 json 配置文件，使用多种获取目录的形式来确认。
        /// 如果存在则返回当前目录的确切路径。
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static string EnsureCurrentDirectory(string json)
        {
            var rootDir = Directory.GetCurrentDirectory();
            var jsonFile = Path.Combine(rootDir, json);
            if (!File.Exists(jsonFile))
            {
                rootDir = AppDomain.CurrentDomain.BaseDirectory;
                jsonFile = Path.Combine(rootDir, json);
                if (!File.Exists(jsonFile))
                {
                    throw new FileNotFoundException("Can not find app config file .", jsonFile);
                }
            }

            return rootDir;
        }


        public static ConfigClientOptions FromLocalAppsettingsOrEmpty(string json = "appsettings.json")
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            var rootDir = EnsureCurrentDirectory(json);

            var localconfig = new ConfigurationBuilder()
                             .SetBasePath(rootDir)
                             .AddJsonFile(json).AddEnvironmentVariables().Build();

            var configSection = localconfig.GetSection("AgileConfig");
            if (!configSection.Exists())
            {
                return null;
            }

            return FromConfiguration(localconfig);
        }

        public static ConfigClientOptions FromLocalAppsettings(string json = "appsettings.json")
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json));

            var rootDir = EnsureCurrentDirectory(json);

            var localconfig = new ConfigurationBuilder()
                             .SetBasePath(rootDir)
                             .AddJsonFile(json).AddEnvironmentVariables().Build();

            return FromConfiguration(localconfig);
        }

        public static ConfigClientOptions FromConfiguration(IConfiguration config)
        {
            var configSection = config.GetSection("AgileConfig");
            if (!configSection.Exists())
            {
                throw new Exception($"Can not find section:AgileConfig from IConfiguration instance .");
            }

            var options = new ConfigClientOptions();

            var appId = config["AgileConfig:appId"];
            var serverNodes = config["AgileConfig:nodes"];

            if (string.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException(nameof(appId));
            }
            if (string.IsNullOrEmpty(serverNodes))
            {
                throw new ArgumentNullException(nameof(serverNodes));
            }
            var secret = config["AgileConfig:secret"];
            var name = config["AgileConfig:name"];
            var tag = config["AgileConfig:tag"];
            var env = config["AgileConfig:env"];
            var timeout = config["AgileConfig:httpTimeout"];
            var cacheDir = config["AgileConfig:cache:directory"] ?? "";

            options.Name = name;
            options.Tag = tag;
            options.AppId = appId;
            options.Secret = secret;
            options.Nodes = serverNodes;
            options.ENV = string.IsNullOrEmpty(env) ? "" : env.ToUpper();
            options.CacheDirectory = cacheDir;
            if (int.TryParse(timeout, out int iTimeout))
            {
                options.HttpTimeout = iTimeout;
            }

            //read service info
            var serviceRegisterConf = config.GetSection("AgileConfig:serviceRegister");
            if (serviceRegisterConf.Exists())
            {
                options.RegisterInfo = new ServiceRegisterInfo();
            }
            else
            {
                return options;
            }

            var serviceId = config["AgileConfig:serviceRegister:serviceId"];
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentNullException("serviceRegister:serviceId");
            }
            var serviceName = config["AgileConfig:serviceRegister:serviceName"];
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException("serviceRegister:serviceName");
            }

            var ip = config["AgileConfig:serviceRegister:ip"];
            var port = config["AgileConfig:serviceRegister:port"];
            var alarmUrl = config["AgileConfig:serviceRegister:alarmUrl"];
            var checkUrl = config["AgileConfig:serviceRegister:heartbeat:url"];
            var mode = config["AgileConfig:serviceRegister:heartbeat:mode"];
            var heartbeatInverval = config["AgileConfig:serviceRegister:heartbeat:interval"];
            var metaData = new List<string>();
            config.GetSection("AgileConfig:serviceRegister:metaData").Bind(metaData);
            options.RegisterInfo.ServiceId = serviceId;
            options.RegisterInfo.ServiceName = serviceName;
            options.RegisterInfo.Ip = ip;
            options.RegisterInfo.CheckUrl = checkUrl;
            options.RegisterInfo.AlarmUrl = alarmUrl;
            if (string.IsNullOrWhiteSpace(mode))
            {
                mode = "client";
            }
            options.RegisterInfo.HeartBeatMode = mode;
            options.RegisterInfo.MetaData = metaData;
            if (int.TryParse(port,out int iport))
            {
                options.RegisterInfo.Port = iport;
            }
            if (int.TryParse(heartbeatInverval, out int IheartbeatInverval))
            {
                options.RegisterInfo.Interval = IheartbeatInverval;
            }

            return options;
        }
    }
}
