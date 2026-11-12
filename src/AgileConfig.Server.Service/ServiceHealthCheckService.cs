using System;
using System.Threading;
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
    private readonly ServiceHealthCheckRecovery _recovery;
    private readonly TimeProvider _timeProvider;

    private int _removeServiceInterval;
    private int _unhealthInterval;

    public ServiceHealthCheckService(
        IServiceInfoService serviceInfoService,
        ILogger<ServiceHealthCheckService> logger,
        IRestClient restClient,
        ServiceHealthCheckRecovery recovery,
        TimeProvider timeProvider
    )
    {
        _serviceInfoService = serviceInfoService;
        _logger = logger;
        _restClient = restClient;
        _recovery = recovery;
        _timeProvider = timeProvider;
    }

    /// <summary>
    ///     Time without a client heartbeat before a service is considered unhealthy.
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

    public async Task CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var services = await _serviceInfoService
                .QueryAsync(x => x.HeartBeatMode != null && x.HeartBeatMode != "");
            var inGracePeriod = _recovery.IsInGracePeriod();
            await Parallel.ForEachAsync(services, new ParallelOptions
            {
                MaxDegreeOfParallelism = 8,
                CancellationToken = cancellationToken
            }, async (service, token) =>
            {
                if (service.HeartBeatMode == HeartBeatModes.none.ToString() ||
                    string.IsNullOrWhiteSpace(service.HeartBeatMode)) return;

                var lastHeartbeat = service.LastHeartBeat ?? service.RegisterTime ?? DateTime.MinValue;
                var now = _timeProvider.GetLocalNow().DateTime;
                if (!inGracePeriod && RemoveServiceInterval > 0 &&
                    (now - lastHeartbeat).TotalSeconds > RemoveServiceInterval)
                {
                    await _serviceInfoService.RemoveAsync(service.Id);
                    return;
                }

                if (service.HeartBeatMode == HeartBeatModes.client.ToString())
                {
                    if (!inGracePeriod && (now - lastHeartbeat).TotalSeconds > UnhealthInterval &&
                        service.Status == ServiceStatus.Healthy)
                        await _serviceInfoService.UpdateServiceStatus(service, ServiceStatus.Unhealthy);
                    return;
                }

                if (service.HeartBeatMode == HeartBeatModes.server.ToString())
                {
                    if (string.IsNullOrWhiteSpace(service.CheckUrl))
                    {
                        await _serviceInfoService.UpdateServiceStatus(service, ServiceStatus.Unhealthy);
                        return;
                    }

                    var result = await CheckAService(service, token);
                    token.ThrowIfCancellationRequested();
                    await _serviceInfoService.UpdateServiceStatus(service,
                        result ? ServiceStatus.Healthy : ServiceStatus.Unhealthy);
                }
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            _recovery.RequireRecovery();
            throw;
        }
    }

    private async Task<bool> CheckAService(ServiceInfo service, CancellationToken cancellationToken)
    {
        try
        {
            using var resp = await _restClient.GetAsync(service.CheckUrl, cancellationToken);

            var result = false;
            var istatus = (int)resp.StatusCode - 200;
            result = istatus >= 0 && istatus < 100; // Treat 2xx status codes as healthy responses.

            if (!result)
                _logger.LogInformation("check service health {0} {1} {2} result：{3}", service.CheckUrl,
                    service.ServiceId,
                    service.ServiceName, "down");

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "check service health {0} {1} {2} error", service.CheckUrl, service.ServiceId,
                service.ServiceName);
            return false;
        }
    }
}