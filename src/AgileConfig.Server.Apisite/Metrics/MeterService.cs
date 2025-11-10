using System;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.ResourceMonitoring;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Apisite.Metrics;

public class MeterService : IMeterService
{
    public const string MeterName = "AgileConfigMeter";

    private const int _checkInterval = 5;

    private readonly IAppService _appService;
    private readonly IConfigService _configService;
    private readonly IRemoteServerNodeProxy _remoteServer;
    private readonly IResourceMonitor _resourceMonitor;
    private readonly IServerNodeService _serverNodeService;
    private readonly IServiceInfoService _serviceInfoService;

    private readonly IServiceScope _serviceScope;

    private int _appCount;
    private int _clientCount;
    private int _configCount;
    private double _cpuUsed;
    private long _memoryUsed;
    private int _serverNodeCount;
    private int _serviceCount;

    static MeterService()
    {
        AgileConfigMeter = new Meter(MeterName, "1.0");
    }

    public MeterService(IServiceScopeFactory sf)
    {
        _serviceScope = sf.CreateScope();
        var sp = _serviceScope.ServiceProvider;

        _appService = sp.GetService<IAppService>();
        _configService = sp.GetService<IConfigService>();
        _serverNodeService = sp.GetService<IServerNodeService>();
        _remoteServer = sp.GetService<IRemoteServerNodeProxy>();
        _serviceInfoService = sp.GetService<IServiceInfoService>();
        try
        {
            _resourceMonitor = sp.GetService<IResourceMonitor>();
        }
        catch
        {
            var logger = sp.GetService<ILoggerFactory>().CreateLogger<MeterService>();
            logger.LogWarning(
                "Failed to get IResourceMonitor, maybe you are using cgroups v2, currently is not supported.");
        }

        AppGauge = AgileConfigMeter.CreateObservableGauge("AppCount", () => { return _appCount; }, "",
            "The number of enabled apps");

        ConfigGauge = AgileConfigMeter.CreateObservableGauge("ConfigCount", () => { return _configCount; }, "",
            "The number of enabled configuration items");

        ServiceGauge = AgileConfigMeter.CreateObservableGauge("ServiceCount", () => { return _serviceCount; }, "",
            "The number of registered services");

        ClientGauge = AgileConfigMeter.CreateObservableGauge("ClientCount", () => { return _clientCount; }, "",
            "The number of connected clients");

        NodeGauge = AgileConfigMeter.CreateObservableGauge("NodeCount", () => { return _serverNodeCount; }, "",
            "The number of nodes");

        MemoryUsedGauge =
            AgileConfigMeter.CreateObservableGauge("MemoryUsed", () => { return _memoryUsed; }, "MB",
                "Memory used (MB)");

        CpuUsedGauge =
            AgileConfigMeter.CreateObservableGauge("CpuUsed", () => { return _cpuUsed; }, "%", "CPU used percent");

        PullAppConfigCounter = AgileConfigMeter.CreateCounter<long>("PullAppConfigCounter", "",
            "The number of times the app configuration was pulled");
    }

    public static Meter AgileConfigMeter { get; }
    public ObservableGauge<long> MemoryUsedGauge { get; }
    public ObservableGauge<double> CpuUsedGauge { get; }

    public ObservableGauge<int> AppGauge { get; }
    public ObservableGauge<int> ConfigGauge { get; }
    public ObservableGauge<int> ServiceGauge { get; }
    public ObservableGauge<int> ClientGauge { get; }
    public ObservableGauge<int> NodeGauge { get; }

    public Counter<long> PullAppConfigCounter { get; }

    public void Start()
    {
        _ = StartCheckAppCountAsync();
    }

    private Task StartCheckAppCountAsync()
    {
        return Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                try
                {
                    _appCount = await _appService.CountEnabledAppsAsync();

                    _configCount = await _configService.CountEnabledConfigsAsync();

                    var services = await _serviceInfoService.GetAllServiceInfoAsync();
                    _serviceCount = services.Count;

                    var nodes = await _serverNodeService.GetAllNodesAsync();
                    _serverNodeCount = nodes.Count;

                    var clientCount = 0;
                    foreach (var item in nodes)
                        if (item.Status == NodeStatus.Online)
                        {
                            var clientInfos = await _remoteServer.GetClientsReportAsync(item.Id);
                            clientCount += clientInfos.ClientCount;
                        }

                    _clientCount = clientCount;

                    // memory and cpu
                    if (_resourceMonitor != null)
                    {
                        var window = TimeSpan.FromSeconds(3);
                        var utilization = _resourceMonitor.GetUtilization(window);
                        _memoryUsed = (long)utilization.MemoryUsedInBytes / 1024 / 1024;
                        _cpuUsed = utilization.CpuUsedPercentage;
                    }
                }
                catch
                {
                }

                await Task.Delay(1000 * _checkInterval);
            }
        }, TaskCreationOptions.LongRunning);
    }
}