using System;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class ServiceHealthCheckService : IServiceHealthCheckService
{
    private readonly ILogger _logger;
    private readonly IRestClient _restClient;
    private readonly IServiceInfoService _serviceInfoService;

    private int _checkInterval;
    private int _removeServiceInterval;
    private int _unhealthInterval;

    public ServiceHealthCheckService(
        IServiceInfoService serviceInfoService,
        ILogger<ServiceHealthCheckService> logger,
        IRestClient restClient
    )
    {
        _serviceInfoService = serviceInfoService;
        _logger = logger;
        _restClient = restClient;
    }

    /// <summary>
    ///     Time threshold (seconds) after which a service is considered unhealthy.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private int CheckInterval
    {
        get
        {
            if (_checkInterval > 0) return _checkInterval;

            var interval = Global.Config["serviceHealthCheckInterval"];
            if (int.TryParse(interval, out var i))
            {
                if (i <= 0) throw new ArgumentException("serviceHealthCheckInterval must be greater than 0");

                _checkInterval = i;
            }

            return _checkInterval;
        }
    }

    /// <summary>
    ///     Interval for running health checks, in seconds.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private int UnhealthInterval
    {
        get
        {
            if (_unhealthInterval > 0) return _unhealthInterval;

            var interval = Global.Config["serviceUnhealthInterval"];
            if (int.TryParse(interval, out var i))
            {
                if (i <= 0) throw new ArgumentException("serviceUnhealthInterval must be greater than 0");

                _unhealthInterval = i;
            }

            return _unhealthInterval;
        }
    }

    private int RemoveServiceInterval
    {
        get
        {
            if (_removeServiceInterval > 0) return _removeServiceInterval;

            var interval = Global.Config["removeServiceInterval"];
            if (int.TryParse(interval, out var i))
            {
                if (i <= 0)
                    _removeServiceInterval = 0;
                else
                    _removeServiceInterval = i;
            }

            return _removeServiceInterval;
        }
    }

    public Task StartCheckAsync()
    {
        _logger.LogInformation("start to service health check");

        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                // Skip services without a configured heartbeat mode.
                var services = await _serviceInfoService
                    .QueryAsync(x => x.HeartBeatMode != null && x.HeartBeatMode != "");
                foreach (var service in services)
                {
                    if (service.HeartBeatMode == HeartBeatModes.none.ToString()) continue;

                    var lstHeartBeat = service.LastHeartBeat;
                    if (!lstHeartBeat.HasValue) lstHeartBeat = service.RegisterTime ?? DateTime.MinValue;

                    // A heartbeat mode is specified.
                    if (!string.IsNullOrWhiteSpace(service.HeartBeatMode))
                    {
                        if (RemoveServiceInterval > 0 &&
                            (DateTime.Now - lstHeartBeat.Value).TotalSeconds > RemoveServiceInterval)
                        {
                            // Remove the service if it has exceeded the configured lifetime.
                            await _serviceInfoService.RemoveAsync(service.Id);
                            continue;
                        }

                        // Client-initiated heartbeats do not require an HTTP health check.
                        if (service.HeartBeatMode == HeartBeatModes.client.ToString())
                        {
                            if ((DateTime.Now - lstHeartBeat.Value).TotalSeconds > UnhealthInterval)
                                // For client heartbeats, treat services as unavailable after UnhealthInterval without heartbeats.
                                if (service.Status == ServiceStatus.Healthy)
                                    await _serviceInfoService.UpdateServiceStatus(service, ServiceStatus.Unhealthy);

                            continue;
                        }

                        // Server-initiated HTTP health check.
                        if (service.HeartBeatMode == HeartBeatModes.server.ToString())
                        {
                            if (string.IsNullOrWhiteSpace(service.CheckUrl))
                            {
                                // Without a CheckUrl, consider the service offline.
                                await _serviceInfoService.UpdateServiceStatus(service, ServiceStatus.Unhealthy);
                                continue;
                            }

                            _ = Task.Run(async () =>
                            {
                                var result = await CheckAService(service);
                                await _serviceInfoService.UpdateServiceStatus(service,
                                    result ? ServiceStatus.Healthy : ServiceStatus.Unhealthy);
                            });
                        }
                    }
                }

                await Task.Delay(CheckInterval * 1000);
            }
        }, TaskCreationOptions.LongRunning);

        return Task.CompletedTask;
    }

    private async Task<bool> CheckAService(ServiceInfo service)
    {
        try
        {
            using var resp = await _restClient.GetAsync(service.CheckUrl);

            var result = false;
            var istatus = (int)resp.StatusCode - 200;
            result = istatus >= 0 && istatus < 100; // Treat 2xx status codes as healthy responses.

            if (!result)
                _logger.LogInformation("check service health {0} {1} {2} result：{3}", service.CheckUrl,
                    service.ServiceId,
                    service.ServiceName, "down");

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "check service health {0} {1} {2} error", service.CheckUrl, service.ServiceId,
                service.ServiceName);
            return false;
        }
    }
}