using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Metrics
{
    public class MeterService
    {
        public const string MeterName = "AgileConfigMeter";

        public static Meter AgileConfigMeter { get; }

        public ObservableGauge<int> AppGauge { get; }
        public ObservableGauge<int> ConfigGauge { get; }
        public ObservableGauge<int> ServiceGauge { get; }
        public ObservableGauge<int> ClientGauge { get; }
        public ObservableGauge<int> NodeGauge { get; }

        private readonly IAppService _appService;
        private readonly IConfigService _configService;
        private readonly IServerNodeService _serverNodeService;
        private readonly IRemoteServerNodeProxy _remoteServer;
        private readonly IServiceInfoService _serviceInfoService;

        private int _appCount = 0;
        private int _configCount = 0;
        private int _clientCount = 0;
        private int _serverNodeCount = 0;
        private int _serviceCount = 0;

        private const int _checkInterval = 30;

        static MeterService()
        {
            AgileConfigMeter = new(MeterName, "1.0");
        }

        public MeterService(IServiceScopeFactory sf)
        {
            var sp = sf.CreateScope().ServiceProvider;
            _appService = sp.GetService<IAppService>();
            _configService = sp.GetService<IConfigService>();
            _serverNodeService = sp.GetService<IServerNodeService>();
            _remoteServer = sp.GetService<IRemoteServerNodeProxy>();
            _serviceInfoService = sp.GetService<IServiceInfoService>();

            AppGauge = AgileConfigMeter.CreateObservableGauge<int>("AppCount", () =>
            {
                return _appCount;
            }, "", "The number of enabled apps");
            ConfigGauge = AgileConfigMeter.CreateObservableGauge<int>("ConfigCount", () =>
            {
                return _configCount;
            }, "", "The number of enabled configuration items");
            ServiceGauge = AgileConfigMeter.CreateObservableGauge<int>("ServiceCount", () =>
            {
                return _serviceCount;
            }, "", "The number of registered services");
            ClientGauge = AgileConfigMeter.CreateObservableGauge<int>("ClientCount", () =>
            {
                return _clientCount;
            }, "", "The number of connected clients");
            NodeGauge = AgileConfigMeter.CreateObservableGauge<int>("NodeCount", () =>
            {
                return _serverNodeCount;
            }, "", "The number of nodes");
        }

        public void Start()
        {
            _ = StartCheckAppCountAsync();
            _ = StartCheckConfigCountAsync();
            _ = StartCheckNodeCountAsync();
            _ = StartCheckNodeCountAsync();
            _ = StartCheckServiceCountAsync();
        }

        private Task StartCheckAppCountAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    _appCount = await _appService.CountEnabledAppsAsync();
                    await Task.Delay(1000 * _checkInterval);
                }
            });
        }

        private Task StartCheckConfigCountAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    _configCount = await _configService.CountEnabledConfigsAsync();
                    await Task.Delay(1000 * _checkInterval);
                }
            });
        }

        private Task StartCheckNodeCountAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var nodes = await _serverNodeService.GetAllNodesAsync();
                    _serverNodeCount = nodes.Count;

                    var clientCount = 0;
                    foreach (var item in nodes)
                    {
                        if (item.Status == NodeStatus.Online)
                        {
                            var clientInfos = await _remoteServer.GetClientsReportAsync(item.Id.ToString());
                            clientCount += clientInfos.ClientCount;
                        }
                    }
                    _clientCount = clientCount;

                    await Task.Delay(1000 * _checkInterval);
                }
            });
        }

        private Task StartCheckServiceCountAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    var services = await _serviceInfoService.GetAllServiceInfoAsync();
                    _serviceCount = services.Count;

                    await Task.Delay(1000 * _checkInterval);
                }
            });
        }
    }
}
