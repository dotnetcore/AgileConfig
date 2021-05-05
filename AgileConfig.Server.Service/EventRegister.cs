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
            TinyEventBus.Instance.Regist(EventKeys.ADD_SYSLOG, ( parm )=> {
                var log = parm as SysLog;
                if (log != null)
                {
                    _sysLogService.AddSysLogAsync(log);
                }
            });
            TinyEventBus.Instance.Regist(EventKeys.ADD_RANGE_SYSLOG, (parm) => {
                var logs = parm as IEnumerable<SysLog>;
                if (logs != null)
                {
                    _sysLogService.AddRangeAsync(logs);
                }
            });
        }
    }
}
