using System;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using AgileHttp;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class ServiceHealthCheckService : IServiceHealthCheckService
{
    private readonly ILogger _logger;
    private readonly IServiceInfoService _serviceInfoService;

    public ServiceHealthCheckService(
        IServiceInfoService serviceInfoService,
        ILogger<ServiceHealthCheckService> logger)
    {
        _serviceInfoService = serviceInfoService;
        _logger = logger;
    }

    private int _checkInterval;
    private int _unhealthInterval;
    private int _removeServiceInterval;

    /// <summary>
    /// 健康检测的间隔
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private int CheckInterval
    {
        get
        {
            if (_checkInterval > 0)
            {
                return _checkInterval;
            }

            var interval = Global.Config["serviceHealthCheckInterval"];
            if (int.TryParse(interval, out int i))
            {
                if (i <= 0)
                {
                    throw new ArgumentException("serviceHealthCheckInterval must be greater than 0");
                }

                _checkInterval = i;
            }

            return _checkInterval;
        }
    }

    /// <summary>
    /// 判断一个服务是否健康的标准时间，操作这个时间没有收到响应，则认为不健康
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    private int UnhealthInterval
    {
        get
        {
            if (_unhealthInterval > 0)
            {
                return _unhealthInterval;
            }

            var interval = Global.Config["serviceUnhealthInterval"];
            if (int.TryParse(interval, out int i))
            {
                if (i <= 0)
                {
                    throw new ArgumentException("serviceUnhealthInterval must be greater than 0");
                }

                _unhealthInterval = i;
            }

            return _unhealthInterval;
        }
    }

    private int RemoveServiceInterval
    {
        get
        {
            if (_removeServiceInterval > 0)
            {
                return _removeServiceInterval;
            }

            var interval = Global.Config["removeServiceInterval"];
            if (int.TryParse(interval, out int i))
            {
                if (i <= 0)
                {
                    _removeServiceInterval = 0;
                }
                else
                {
                    _removeServiceInterval = i;
                }
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
                //没有填写心跳模式，则不做检查
                var services = await FreeSQL.Instance.Select<ServiceInfo>()
                    .Where(x => x.HeartBeatMode != null && x.HeartBeatMode != "").ToListAsync();
                foreach (var service in services)
                {
                    if (service.HeartBeatMode == HeartBeatModes.none.ToString())
                    {
                        continue;
                    }
                    
                    var lstHeartBeat = service.LastHeartBeat;
                    if (!lstHeartBeat.HasValue)
                    {
                        lstHeartBeat = service.RegisterTime ?? DateTime.MinValue;
                    }

                    //service.HeartBeatMode 不为空
                    if (!string.IsNullOrWhiteSpace(service.HeartBeatMode))
                    {
                        if (RemoveServiceInterval > 0 &&
                            (DateTime.Now - lstHeartBeat.Value).TotalSeconds > RemoveServiceInterval)
                        {
                            //超过设定时间，则直接删除服务
                            await _serviceInfoService.RemoveAsync(service.Id);
                            continue;
                        }

                        //是客户端主动心跳，不做http健康检查
                        if (service.HeartBeatMode == HeartBeatModes.client.ToString())
                        {
                            if ((DateTime.Now - lstHeartBeat.Value).TotalSeconds > UnhealthInterval)
                            {
                                //客户端主动心跳模式：超过 UnhealthInterval 没有心跳，则认为服务不可用
                                if (service.Status == ServiceStatus.Healthy)
                                {
                                    await _serviceInfoService.UpdateServiceStatus(service, ServiceStatus.Unhealthy);
                                }
                            }

                            continue;
                        }

                        //等于server 主动http健康检查
                        if (service.HeartBeatMode == HeartBeatModes.server.ToString())
                        {
                            if (string.IsNullOrWhiteSpace(service.CheckUrl))
                            {
                                //CheckUrl不填，直接认为下线
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
            using var resp = await service.CheckUrl
                .AsHttp()
                .SendAsync();
            if (resp.Exception != null)
            {
                throw resp.Exception;
            }

            var result = false;
            if (resp.StatusCode.HasValue)
            {
                int istatus = ((int)resp.StatusCode - 200);
                result = istatus >= 0 && istatus < 100; // 200 段都认为是正常的
            }

            _logger.LogInformation("check service health {0} {1} {2} result：{3}", service.CheckUrl, service.ServiceId,
                service.ServiceName, result ? "up" : "down");
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