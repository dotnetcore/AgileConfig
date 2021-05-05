using AgileConfig.Server.Common;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Data.Freesql;
using AgileConfig.Server.IService;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Service
{
    public class EventRegister : IEventRegister
    {

        private ISysLogService _sysLogService;

        public EventRegister()
        {
            _sysLogService = new SysLogService(new FreeSqlContext(FreeSQL.Instance));
        }

        public void Init()
        {
            RegisterAddSysLog();
        }

        /// <summary>
        /// 注册添加系统日志事件
        /// </summary>
        private void RegisterAddSysLog()
        {
            TinyEventBus.Instance.Regist(EventKeys.ADMIN_LOGIN_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"管理员登录成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.INIT_ADMIN_PASSWORD_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"管理员密码初始化成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.RESET_ADMIN_PASSWORD_SUCCESS, (parm) =>
            {
                var log = new SysLog
                {
                    LogTime = DateTime.Now,
                    LogType = SysLogType.Normal,
                    LogText = $"修改管理员密码成功"
                };
                _sysLogService.AddSysLogAsync(log);
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"新增应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.EDIT_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"编辑应用【AppId：{app.Id}】【AppName：{app.Name}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DISABLE_OR_ENABLE_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"{(app.Enabled ? "启用" : "禁用")}应用【AppId:{app.Id}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.DELETE_APP_SUCCESS, (param) =>
            {
                var app = param as App;
                if (app != null)
                {
                    var log = new SysLog
                    {
                        LogTime = DateTime.Now,
                        LogType = SysLogType.Normal,
                        LogText = $"删除应用【AppId:{app.Id}】"
                    };
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_SYSLOG, (parm) =>
            {
                var log = parm as SysLog;
                if (log != null)
                {
                    _sysLogService.AddSysLogAsync(log);
                }
            });

            TinyEventBus.Instance.Regist(EventKeys.ADD_RANGE_SYSLOG, (parm) =>
            {
                var logs = parm as IEnumerable<SysLog>;
                if (logs != null)
                {
                    _sysLogService.AddRangeAsync(logs);
                }
            });
        }
    }
}
