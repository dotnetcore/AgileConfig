using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Logging;

namespace AgileConfig.Server.Service;

public class RemoteServerNodeProxy : IRemoteServerNodeProxy
{
    private static readonly Dictionary<string, string> _modules = new()
    {
        { "r", "Registration Center" },
        { "c", "Configuration Center" }
    };

    private readonly ILogger _logger;
    private readonly IRestClient _restClient;
    private readonly IServerNodeService _serverNodeService;
    private readonly ISysLogService _sysLogService;

    public RemoteServerNodeProxy(
        ILoggerFactory loggerFactory,
        IRestClient restClient,
        IServerNodeService serverNodeService,
        ISysLogService sysLogService)
    {
        _logger = loggerFactory.CreateLogger<RemoteServerNodeProxy>();
        _restClient = restClient;
        _serverNodeService = serverNodeService;
        _sysLogService = sysLogService;
    }

    public async Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action)
    {
        var result = await FunctionUtil.TRYAsync(async () =>
        {
            var result = await _restClient.PostAsync<dynamic>(address + "/RemoteOP/AllClientsDoAction", action);
            if ((bool)result.success) return true;

            return false;
        }, 5);

        _modules.TryGetValue(action.Module, out var moduleName);
        await _sysLogService.AddSysLogAsync(new SysLog
        {
            LogTime = DateTime.Now,
            LogType = result ? SysLogType.Normal : SysLogType.Warn,
            LogText =
                $"Notified node [{address}] all clients: [{moduleName}] [{action.Action}] Response: {(result ? "Success" : "Failed")}"
        });

        return result;
    }

    public async Task<bool> AppClientsDoActionAsync(string address, string appId, string env, WebsocketAction action)
    {
        var result = await FunctionUtil.TRYAsync(async () =>
        {
            var url = $"{address}/RemoteOP/AppClientsDoAction?appId={Uri.EscapeDataString(appId)}&env={env}";
            var result = await _restClient.PostAsync<dynamic>(url, action);
            if ((bool)result.success) return true;

            return false;
        }, 5);

        _modules.TryGetValue(action.Module, out var moduleName);
        await _sysLogService.AddSysLogAsync(new SysLog
        {
            LogTime = DateTime.Now,
            LogType = result ? SysLogType.Normal : SysLogType.Warn,
            AppId = appId,
            LogText =
                $"Notified node [{address}] app [{appId}] clients: [{moduleName}] [{action.Action}] Response: {(result ? "Success" : "Failed")}"
        });
        return result;
    }

    public async Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action)
    {
        var result = await FunctionUtil.TRYAsync(async () =>
        {
            var url = $"{address}/RemoteOP/OneClientDoAction?clientId={clientId}";
            var result = await _restClient.PostAsync<dynamic>(url, action);

            return (bool)result.success;
        }, 5);

        _modules.TryGetValue(action.Module, out var moduleName);
        await _sysLogService.AddSysLogAsync(new SysLog
        {
            LogTime = DateTime.Now,
            LogType = result ? SysLogType.Normal : SysLogType.Warn,
            LogText =
                $"Notified node [{address}] client [{clientId}]: [{moduleName}] [{action.Action}] Response: {(result ? "Success" : "Failed")}"
        });

        return result;
    }

    public async Task<ClientInfos> GetClientsReportAsync(string address)
    {
        if (string.IsNullOrEmpty(address))
            return new ClientInfos
            {
                ClientCount = 0,
                Infos = new List<ClientInfo>()
            };

        try
        {
            var url = address + "/report/Clients";

            var clients = await _restClient.GetAsync<ClientInfos>(url);
            if (clients != null)
            {
                clients.Infos?.ForEach(i => { i.Address = address; });
                return clients;
            }

            return new ClientInfos
            {
                ClientCount = 0,
                Infos = new List<ClientInfo>()
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Try to get client infos from node {address} occur ERROR . ", ex);
        }

        return new ClientInfos
        {
            ClientCount = 0,
            Infos = new List<ClientInfo>()
        };
    }

    public Task TestEchoAsync(string address)
    {
        return TestEchoAsync(address, CancellationToken.None);
    }

    private async Task TestEchoAsync(string address, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var node = await _serverNodeService.GetAsync(address);
        if (node == null) return; // Another node may have removed it since the last scan.
        try
        {
            var url = node.Id + "/home/echo";

            using var resp = await _restClient.GetAsync(url, cancellationToken);

            if (resp.StatusCode == HttpStatusCode.OK && await resp.Content.ReadAsStringAsync(cancellationToken) == "ok")
            {
                node.LastEchoTime = DateTime.Now;
                node.Status = NodeStatus.Online;
            }
            else
            {
                node.Status = NodeStatus.Offline;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            node.Status = NodeStatus.Offline;
            _logger.LogInformation(e, "Try test node {0} echo , but fail .", node.Id);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (node.Status == NodeStatus.Offline)
        {
            var time = node.LastEchoTime;
            if (!time.HasValue) time = node.CreateTime;
            if (time.HasValue && (DateTime.Now - time.Value).TotalMinutes >= 30)
            {
                // Remove nodes that have not responded for over 30 minutes.
                await _serverNodeService.DeleteAsync(address);
                return;
            }
        }

        await _serverNodeService.UpdateAsync(node);
    }

    public Task TestEchoAsync()
    {
        return TestEchoAsync(CancellationToken.None);
    }

    public async Task TestEchoAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var nodes = await _serverNodeService.GetAllNodesAsync();
        foreach (var node in nodes) await TestEchoAsync(node.Id, cancellationToken);
    }

    public async Task ClearConfigServiceCache(string address)
    {
        try
        {
            var url = address + "/RemoteOP/ClearConfigServiceCache";
            using var resp = await _restClient.PostAsync(url, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Try to clear node {0}'s config cache , but fail .", address);
        }
    }

    public async Task ClearServiceInfoCache(string address)
    {
        try
        {
            var url = address + "/RemoteOP/ClearServiceInfoCache";

            await _restClient.PostAsync(url, null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Try to clear node {0}'s servicesinfo cache , but fail .", address);
        }
    }
}