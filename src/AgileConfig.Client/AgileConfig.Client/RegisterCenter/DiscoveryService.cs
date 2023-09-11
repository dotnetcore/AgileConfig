using AgileConfig.Client.MessageHandlers;
using AgileConfig.Protocol;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Client.RegisterCenter
{
    public class DiscoveryService : IDiscoveryService
    {
        private List<ServiceInfo> _services;
        private IConfigClient _configClient;
        private ILogger _logger;
        private ConfigClientOptions _options
        {
            get
            {
                return _configClient.Options;
            }
        }
        private bool _isLoadFromLocal;

        private string LocalCacheFileName => Path.Combine(_options?.CacheDirectory, $"{_options?.AppId}.agileconfig.client.services.cache");

        public DiscoveryService(IConfigClient client, ILoggerFactory loggerFactory)
        {
            Instance = this;

            _services = new List<ServiceInfo>();
            _configClient = client;
            _logger = loggerFactory.CreateLogger<DiscoveryService>();
            RefreshAsync().GetAwaiter().GetResult();
            MessageCenter.Subscribe += (str) =>
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return;
                }

                try
                {
                    if (RegisterCenterActionMessageHandler.Hit(str))
                    {
                        var act = JsonConvert.DeserializeObject<ActionMessage>(str);
                        if (act == null)
                        {
                            return;
                        }

                        if (act.Action == ActionConst.Reload)
                        {
                            _ = RefreshAsync();
                            return;
                        }
                        if (act.Action == ActionConst.Ping)
                        {
                            var ver = act.Data ?? "";
                            if (!ver.Equals(DataVersion, StringComparison.CurrentCultureIgnoreCase))
                            {
                                _logger.LogInformation($"server return service infos version {ver} is different from local version {DataVersion} so refresh .");
                                //如果服务端跟客户端的版本不一样直接刷新
                                _ = RefreshAsync();
                            }
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"DiscoveryService handle receive msg error . message: {str}");
                }
            };
        }

        /// <summary>
        /// 是否读取的事本地缓存的服务
        /// </summary>
        public bool IsLoadFromLocal
        {
            get
            {
                return _isLoadFromLocal;
            }
        }

        public string DataVersion { get; private set; }


        public List<ServiceInfo> Services
        {
            get
            {
                return _services;
            }
        }

        public List<ServiceInfo> HealthyServices
        {
            get
            {
                return _services.Where(x => x.Status == ServiceStatus.Healthy).ToList();
            }
        }

        public List<ServiceInfo> UnHealthyServices
        {
            get
            {
                return _services.Where(x => x.Status == ServiceStatus.Unhealthy).ToList();
            }
        }

        public static IDiscoveryService Instance
        {
            get; private set;
        }

        public async Task RefreshAsync()
        {
            int failCount = 0;
            var random = new RandomServers(_configClient.Options.Nodes);
            while (!random.IsComplete)
            {   //随机一个节点尝试移除
                var host = random.Next();
                var getUrl = host + (host.EndsWith("/") ? "" : "/") + $"api/registercenter/services";
                try
                {
                    var resp = await HttpUtil.GetAsync(getUrl, null, null);

                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await HttpUtil.GetResponseContentAsync(resp);
                        if (!string.IsNullOrEmpty(content))
                        {
                            var result = JsonConvert.DeserializeObject<List<ServiceInfo>>(content);
                            if (result != null)
                            {
                                this._isLoadFromLocal = false;
                                this._services = result;
                                this.DataVersion = GenerateMD5(result);
                                WriteServiceInfosToLocal(content);
                                _logger.LogTrace($"DiscoveryService refresh all services success by API {getUrl} , status code {resp.StatusCode} .");
                            }
                        }
                        break;
                    }
                    else
                    {
                        _logger.LogTrace($"DiscoveryService refresh all services fail , url {getUrl} , status code {resp.StatusCode} .");
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    _logger.LogError(ex, "DiscoveryService refresh all services error .");
                }
            }
            if (failCount == random.ServerCount)
            {
                LoadServicesFromLocal();
            }
        }

        /// <summary>
        /// 从本地缓存文件加载服务信息
        /// </summary>
        private void LoadServicesFromLocal()
        {
            var fileContent = ReadServiceInfosContentFromLocal();
            if (!string.IsNullOrEmpty(fileContent))
            {
                var result = JsonConvert.DeserializeObject<List<ServiceInfo>>(fileContent);
                if (result != null)
                {
                    this._services = result;
                    this.DataVersion = GenerateMD5(result);
                    this._isLoadFromLocal = true;

                    _logger?.LogTrace("client load all service infos from local file .");
                }
            }
        }

        /// <summary>
        /// 保证cache文件夹存在
        /// </summary>
        private void EnsureCacheDir()
        {
            if (!string.IsNullOrWhiteSpace(_options.CacheDirectory) && !Directory.Exists(_options.CacheDirectory))
            {
                Directory.CreateDirectory(_options.CacheDirectory);
            }
        }

        private void WriteServiceInfosToLocal(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    return;
                }
                EnsureCacheDir();
                File.WriteAllText(LocalCacheFileName, content);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "client try to cache all service infos to local but failed .");
            }
        }

        /// <summary>
        /// 尝试从本地文件读取缓存的服务信息
        /// </summary>
        /// <returns></returns>
        private string ReadServiceInfosContentFromLocal()
        {
            EnsureCacheDir();
            if (!File.Exists(LocalCacheFileName))
            {
                return "";
            }

            return File.ReadAllText(LocalCacheFileName);
        }

        private string GenerateMD5(List<ServiceInfo> services)
        {
            var plain = new StringBuilder();
            foreach (var serviceInfo in services.OrderBy(x => x.ServiceId))
            {
                var metaDataStr = "";
                if (serviceInfo.MetaData != null)
                {
                    metaDataStr = string.Join(",", serviceInfo.MetaData.OrderBy(x => x));
                }
                plain.Append($"{serviceInfo.ServiceId}&{serviceInfo.ServiceName}&{serviceInfo.Ip}&{serviceInfo.Port}&{(int)serviceInfo.Status}&{metaDataStr}&");
            }

            var txt = plain.ToString();
            var md5 = Encrypt.Md5(txt);

            return md5;
        }

    }
}
