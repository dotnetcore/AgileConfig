﻿using System;
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

    private int _interval;

    private int Interval
    {
        get
        {
            if (_interval > 0)
            {
                return _interval;
            }

            var interval = Global.Config["serviceHealthCheckInterval"];
            if (int.TryParse(interval, out int i))
            {
                if (i <= 0)
                {
                    throw new ArgumentException("serviceHealthCheckInterval must be greater than 0");
                }

                _interval = i;
            }

            return _interval;
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
                    var lstHeartBeat = service.LastHeartBeat;
                    if (!lstHeartBeat.HasValue)
                    {
                        lstHeartBeat = service.RegisterTime ?? DateTime.MinValue;
                    }

                    // if ((DateTime.Now - lstHeartBeat.Value).TotalMinutes > 10)
                    // {
                    //     //超过10分钟没有心跳，则直接删除服务
                    //     await RemoveService(service.Id);
                    //     continue;
                    // }

                    //service.HeartBeatMode 不为空，且不等于server 则认为是客户端主动心跳，不做http健康检查
                    if (!string.IsNullOrWhiteSpace(service.HeartBeatMode) && service.HeartBeatMode != "server")
                    {
                        if ((DateTime.Now - lstHeartBeat.Value).TotalMinutes > 1)
                        {
                            //客户端主动心跳模式：超过1分钟没有心跳，则认为服务不可用
                            if (service.Alive == ServiceAlive.Online)
                            {
                                await _serviceInfoService.UpdateServiceStatus(service, ServiceAlive.Offline);
                            }
                        }

                        continue;
                    }

                    //service.HeartBeatMode 不为空，且 = server
                    if (!string.IsNullOrWhiteSpace(service.HeartBeatMode) && service.HeartBeatMode == "server")
                    {
                        if (string.IsNullOrWhiteSpace(service.CheckUrl))
                        {
                            //CheckUrl不填，直接认为下线
                            await _serviceInfoService.UpdateServiceStatus(service, ServiceAlive.Offline);
                            continue;
                        }

                        //这个 task 没必要等待，没必要等待上一个service检测结束开始下一个。
#pragma warning disable CS4014
                        Task.Run(async () =>
#pragma warning restore CS4014
                        {
                            var result = await CheckAService(service);
                            await _serviceInfoService.UpdateServiceStatus(service, result ? ServiceAlive.Online : ServiceAlive.Offline);
                        });
                    }
                }

                await Task.Delay(Interval * 1000);
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