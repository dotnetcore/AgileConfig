using Agile.Config.Protocol;
using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using AgileHttp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AgileConfig.Server.Service
{
    public class RemoteServerNodeProxy : IRemoteServerNodeActionProxy
    {
        private ISysLogService _sysLogService;
        public RemoteServerNodeProxy (ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        public async Task<bool> AllClientsDoActionAsync(string address, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/AllClientsDoAction")
                        .AsHttp("POST", action)
                        .Config(new RequestOptions { ContentType = "application/json" })
                        .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool)result.success)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }, 5);

            await _sysLogService.AddSysLogSync(new SysLog
            {
                LogTime = DateTime.Now,
                LogType = result ? SysLogType.Normal : SysLogType.Warn,
                LogText = $"通知节点{address}所有客户端：{action.Action} 响应：{(result ? "成功" : "失败")}"
            });

            return result;
        }

        public async Task<bool> AppClientsDoActionAsync(string address, string appId, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/AppClientsDoAction".AppendQueryString("appId", appId))
                                       .AsHttp("POST", action)
                                       .Config(new RequestOptions { ContentType = "application/json" })
                                       .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool)result.success)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }, 5);

            await _sysLogService.AddSysLogSync(new SysLog
            {
                LogTime = DateTime.Now,
                LogType = result ? SysLogType.Normal : SysLogType.Warn,
                AppId = appId,
                LogText = $"通知节点{address}应用{appId}的客户端：{action.Action} 响应：{(result ? "成功" : "失败")}"
            });

            return result;
        }

        public async Task<bool> OneClientDoActionAsync(string address, string clientId, WebsocketAction action)
        {
            var result = await FunctionUtil.TRY(async () =>
            {
                using (var resp = await (address + "/RemoteOP/OneClientDoAction?clientId=" + clientId)
                            .AsHttp("POST", action)
                            .Config(new RequestOptions { ContentType = "application/json" })
                            .SendAsync())
                {
                    if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = resp.Deserialize<dynamic>();

                        if ((bool)result.success)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }, 5);

            await _sysLogService.AddSysLogSync(new SysLog
            {
                LogTime = DateTime.Now,
                LogType = result ? SysLogType.Normal : SysLogType.Warn,
                LogText = $"通知节点{address}的客户端{clientId}：{action.Action} 响应：{(result ? "成功" : "失败")}"
            });

            return result;
        }
    }
}
