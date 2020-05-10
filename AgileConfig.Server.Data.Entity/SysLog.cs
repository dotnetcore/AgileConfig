using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum SysLogType
    {
        Normal =0,
        Warn =1
    }

    [Table("sys_log")]
    public class SysLog
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("app_id")]
        public string AppId { get; set; }

        [Column("log_type")]
        public SysLogType LogType { get; set; }

        [Column("log_time")]
        public DateTime? LogTime { get; set; }

        [Column("log_text")]
        public string LogText { get; set; }
    }
}
