using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum SysLogType
    {
        Normal = 0,
        Warn = 1
    }

    [Table(Name = "sys_log")]
    public class SysLog
    {
        [Column(Name = "id", IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "app_id", DbType = "varchar(36)")]
        public string AppId { get; set; }

        [Column(Name = "log_type")]
        public SysLogType LogType { get; set; }

        [Column(Name = "log_time")]
        public DateTime? LogTime { get; set; }

        [Column(Name = "log_text")]
        public string LogText { get; set; }
    }
}
