using System;
using System.Net;
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
    private ILogger _logger;

    public ServiceHealthCheckService(ILogger<ServiceHealthCheckService> logger)
    {
        _logger = logger;
    }

    private int Interval
    {
        get
        {
            var interval = Global.Config["serviceHealthCheckInterval"];
            if (int.TryParse(interval,out int i))
            {
                return i;
            }

            return 30;
        }
    }
    
    public Task StartCheckAsync()
    {
        _logger.LogInformation("start to service health check");

        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                var services = await FreeSQL.Instance.Select<ServiceInfo>().Where(x=>x.CheckUrl != null).ToListAsync();
                foreach (var service in services)
                {
                    //service.HeartBeatMode 不为空，且不等于server 则认为是客户端主动心跳，不做http健康检查
                    if (!string.IsNullOrWhiteSpace(service.HeartBeatMode) && service.HeartBeatMode != "server")
                    {
                        var lstHeartBeat = service.LastHeartBeat;
                        if (lstHeartBeat == null)
                        {
                            lstHeartBeat = DateTime.Now;
                        }
                        if ((DateTime.Now - lstHeartBeat.Value).TotalMinutes > 10)
                        {
                            //客户端主动心跳模式：超过10分钟没有心跳，则认为服务不可用
                            await UpdateServiceStatus(service.Id, ServiceAlive.Offline);
                        }
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(service.CheckUrl))
                    {
                        continue;
                    }

                    //这个 task 没必要等待，没必要等待上一个service检测结束开始下一个。
#pragma warning disable CS4014
                    Task.Run(async () =>
#pragma warning restore CS4014
                    {
                        var result = await CheckAService(service);
                        await UpdateServiceStatus(service.Id, result ? ServiceAlive.Online : ServiceAlive.Offline);
                    });
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
            var result = resp.StatusCode == HttpStatusCode.OK;
            _logger.LogInformation("check service health {0} {1} {2} result：{3}", service.CheckUrl, service.ServiceId, service.ServiceName, result ? "up" : "down");
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "check service health {0} {1} {2} error",service.CheckUrl, service.ServiceId, service.ServiceName );
            return false;
        }
    }

    private async Task UpdateServiceStatus(string id, ServiceAlive status)
    {
        if (status == ServiceAlive.Offline)
        {
            await FreeSQL.Instance.Update<ServiceInfo>()
                .Set(x => x.Alive, status)
                .Where(x => x.Id == id)
                .ExecuteAffrowsAsync();
        }
        else
        {
            await FreeSQL.Instance.Update<ServiceInfo>()
                .Set(x => x.Alive, status)
                .Set(x => x.LastHeartBeat, DateTime.Now)
                .Where(x => x.Id == id)
                .ExecuteAffrowsAsync();
        }
    }
}