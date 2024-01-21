using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.RestClient;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RemoteServerNodeProxy : IRemoteServerNodeProxy
    {
        // private IServerNodeService GetServerNodeService()
        // {
        //     return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));
        // }
        //
        // private ISysLogService GetSysLogService()
        // {
        //     return new SysLogService(new FreeSqlContext(FreeSQL.Instance));
        // }

        private readonly ILogger _logger;
        private readonly IRestClient _restClient;
        private readonly EventRegisterTransient<IServerNodeService> _serverNodeService;
        private readonly EventRegisterTransient<ISysLogService> _sysLogService;

        private static ConcurrentDictionary<string, ClientInfos> _serverNodeClientReports =
            new ConcurrentDictionary<string, ClientInfos>();
        

        public RemoteServerNodeProxy(
            ILoggerFactory loggerFactory, 
            IRestClient restClient,
            EventRegisterTransient<IServerNodeService> serverNodeService,
            EventRegisterTransient<ISysLogService> sysLogService)
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
                dynamic result = await _restClient.PostAsync<dynamic>(address + "/RemoteOP/AllClientsDoAction", action);
                if ((bool)result.success)
                {
                    return true;
                }

                return false;
            }, 5);

            using (var service = _sysLogService())
            {
                var module = "";
                if (action.Module == "r")
                {
                    module = "注册中心";
                }
                if (action.Module == "c")
                {
                    module = "配置中心";
                }
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    LogText = $"通知节点【{address}】所有客户端：【{module}】【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public async Task<bool> AppClientsDoActionAsync(string address, string appId, string env, WebsocketAction action)
        {
            var result = await FunctionUtil.TRYAsync(async () =>
            {
                var url = $"{address}/RemoteOP/AppClientsDoAction?appId={Uri.EscapeDataString(appId)}&env={env}";
                dynamic result = await _restClient.PostAsync<dynamic>(url, action);
                if ((bool)result.success)
                {
                    return true;
                }

                return false;
            }, 5);

            using (var service = _sysLogService())
            {
                var module = "";
                if (action.Module == "r")
                {
                    module = "注册中心";
                }
                if (action.Module == "c")
                {
                    module = "配置中心";
                }
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    AppId = appId,
                    LogText = $"通知节点【{address}】应用【{appId}】的客户端：【{module}】【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public async Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action)
        {
            var result = await FunctionUtil.TRYAsync(async () =>
            {
                var url = $"{address}/RemoteOP/OneClientDoAction?clientId={clientId}";
                dynamic result = await _restClient.PostAsync<dynamic>(url, action);

                if ((bool)result.success)
                {
                    if (action.Action == ActionConst.Offline)
                    {
                        if (_serverNodeClientReports.ContainsKey(address))
                        {
                            if (_serverNodeClientReports[address].Infos != null)
                            {
                                var report = _serverNodeClientReports[address];
                                report.Infos.RemoveAll(c => c.Id == clientId);
                                report.ClientCount = report.Infos.Count;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }, 5);

            using (var service = _sysLogService())
            {
                var module = "";
                if (action.Module == "r")
                {
                    module = "注册中心";
                }
                if (action.Module == "c")
                {
                    module = "配置中心";
                }
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    LogText = $"通知节点【{address}】的客户端【{clientId}】：【{module}】【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public async Task<ClientInfos> GetClientsReportAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return new ClientInfos()
                {
                    ClientCount = 0,
                    Infos = new List<ClientInfo>()
                };
            }

            try
            {
                var url = address + "/report/Clients";

                var clients = await _restClient.GetAsync<ClientInfos>(url);
                if (clients != null)
                {
                    clients.Infos?.ForEach(i => { i.Address = address; });
                    return clients;
                }

                return new ClientInfos()
                {
                    ClientCount = 0,
                    Infos = new List<ClientInfo>()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Try to get client infos from node {address} occur ERROR . ", ex);
            }

            return new ClientInfos()
            {
                ClientCount = 0,
                Infos = new List<ClientInfo>()
            };
        }

        public async Task TestEchoAsync(string address)
        {
            using var service = _serverNodeService();
            var node = await service.GetAsync(address);
            try
            {
                var url = node.Id + "/home/echo";

                using var resp = await _restClient.GetAsync(url);

                if (resp.StatusCode == System.Net.HttpStatusCode.OK && (await resp.Content.ReadAsStringAsync()) == "ok")
                {
                    node.LastEchoTime = DateTime.Now;
                    node.Status = NodeStatus.Online;
                }
                else
                {
                    node.Status = NodeStatus.Offline;
                }
            }
            catch (Exception e)
            {
                node.Status = NodeStatus.Offline;
                _logger.LogInformation(e, "Try test node {0} echo , but fail .", node.Id);
            }
            
            if (node.Status == NodeStatus.Offline)
            {
                DateTime? time = node.LastEchoTime;
                if (!time.HasValue)
                {
                    time = node.CreateTime;
                }
                if (time.HasValue && (DateTime.Now - time.Value).TotalMinutes >= 30)
                {
                    // 超过 30 分钟没有回应，则移除节点
                    await service.DeleteAsync(address);
                    return;
                }
            }
            
            await service.UpdateAsync(node);
        }

        public Task TestEchoAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    using var service = _serverNodeService();
                    var nodes = await service.GetAllNodesAsync();

                    foreach (var node in nodes)
                    {
                        await TestEchoAsync(node.Id);
                    }

                    await Task.Delay(5000 * 1);
                }
            });
        }

        public async Task ClearConfigServiceCache(string address)
        {
            try
            {
                var url = (address + "/RemoteOP/ClearConfigServiceCache");
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
                var url = (address + "/RemoteOP/ClearServiceInfoCache");

                await _restClient.PostAsync(url, null);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Try to clear node {0}'s servicesinfo cache , but fail .", address);
            }
        }
    }
}