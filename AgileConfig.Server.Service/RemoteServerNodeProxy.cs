﻿using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using AgileHttp;
using AgileHttp.serialize;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RemoteServerNodeProxy : IRemoteServerNodeProxy
    {
        internal class SerializeProvider : ISerializeProvider
        {
            public T Deserialize<T>(string content)
            {
                return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        private IServerNodeService GetGerverNodeService()
        {
            return new ServerNodeService(new FreeSqlContext(FreeSQL.Instance));
        }

        private ILogger _logger;

        private ISysLogService GetSysLogService()
        {
            return new SysLogService(new FreeSqlContext(FreeSQL.Instance));
        }

        private static ConcurrentDictionary<string, ClientInfos> _serverNodeClientReports =
            new ConcurrentDictionary<string, ClientInfos>();

        public RemoteServerNodeProxy(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RemoteServerNodeProxy>();
        }

        public async Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/AllClientsDoAction")
                    .AsHttp("POST", action)
                    .Config(new RequestOptions {ContentType = "application/json"})
                    .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool) result.success)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }, 5);

            using (var service = GetSysLogService())
            {
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    LogText = $"通知节点【{address}】所有客户端：【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public async Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/AppClientsDoAction".AppendQueryString("appId", appId))
                    .AsHttp("POST", action)
                    .Config(new RequestOptions {ContentType = "application/json"})
                    .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool) result.success)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }, 5);

            using (var service = GetSysLogService())
            {
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    AppId = appId,
                    LogText = $"通知节点【{address}】应用【{appId}】的客户端：【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public async Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/OneClientDoAction?clientId=" + clientId)
                    .AsHttp("POST", action)
                    .Config(new RequestOptions {ContentType = "application/json"})
                    .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool) result.success)
                        {
                            if (action.Action == ActionConst.Offline || action.Action == ActionConst.Remove)
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
                    }

                    return false;
                }
            }, 5);

            using (var service = GetSysLogService())
            {
                await service.AddSysLogAsync(new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = result ? SysLogType.Normal : SysLogType.Warn,
                    LogText = $"通知节点【{address}】的客户端【{clientId}】：【{action.Action}】 响应：{(result ? "成功" : "失败")}"
                });
            }

            return result;
        }

        public ClientInfos GetClientsReport(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            _serverNodeClientReports.TryGetValue(address, out ClientInfos report);
            if (report != null)
            {
                report.Infos?.ForEach(i => { i.Address = address; });
            }

            return report;
        }

        public async Task TestEchoAsync(string address)
        {
            using var service = GetGerverNodeService();
            var node = await service.GetAsync(address);
            try
            {
                await FunctionUtil.TRY(async () =>
                {
                    using var resp = (node.Address + "/home/echo").AsHttp().Send();
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK && resp.GetResponseContent() == "ok")
                    {
                        node.LastEchoTime = DateTime.Now;
                        node.Status = Data.Entity.NodeStatus.Online;
                        var report = GetClientReport(node);
                        if (report != null)
                        {
                            if (_serverNodeClientReports.ContainsKey(node.Address))
                            {
                                _serverNodeClientReports[node.Address] = report;
                            }
                            else
                            {
                                _serverNodeClientReports.AddOrUpdate(node.Address, report, (k, r) => report);
                            }
                        }
                    }
                    else
                    {
                        node.Status = Data.Entity.NodeStatus.Offline;
                    }

                    await service.UpdateAsync(node);
                }, 5);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Try test node {0} echo , but fail .", node.Address);
            }
        }

        public Task TestEchoAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    using var service = GetGerverNodeService();
                    var nodes = await service.GetAllNodesAsync();
                    nodes.ForEach(n =>
                    {
                        try
                        {
                            FunctionUtil.TRY(async () =>
                            {
                                using var resp = (n.Address + "/home/echo").AsHttp().Send();
                                if (resp.StatusCode == HttpStatusCode.OK && resp.GetResponseContent() == "ok")
                                {
                                    n.LastEchoTime = DateTime.Now;
                                    n.Status = NodeStatus.Online;
                                    var report = GetClientReport(n);
                                    if (_serverNodeClientReports.ContainsKey(n.Address))
                                    {
                                        _serverNodeClientReports[n.Address] = report;
                                    }
                                    else
                                    {
                                        _serverNodeClientReports.AddOrUpdate(n.Address, report,
                                            (k, r) => report);
                                    }
                                }
                                else
                                {
                                    n.Status = NodeStatus.Offline;
                                }

                                await service.UpdateAsync(n);
                            }, 5);
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation(e, "Try test node {0} echo , but fail .", n.Address);
                        }
                    });

                    await Task.Delay(5000 * 1);
                }
            });
        }

        private ClientInfos GetClientReport(ServerNode node)
        {
            return FunctionUtil.TRY(() =>
            {
                using (var resp = (node.Address + "/report/Clients").AsHttp()
                    .Config(new RequestOptions(new SerializeProvider())).Send())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = resp.GetResponseContent();
                        _logger.LogTrace($"ServerNode: {node.Address} report clients infomation , {content}");

                        var report = resp.Deserialize<ClientInfos>();
                        if (report != null)
                        {
                            return report;
                        }
                    }

                    return null;
                }
            }, 5);
        }
    }
}