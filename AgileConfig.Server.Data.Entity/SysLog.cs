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

    [Table(Name = "agc_service_info")]
    [OraclePrimaryKeyName("agc_sys_servieinfo_pk")]
    public class ServiceInfo
    {
        [Column(Name = "id", IsIdentity = true)]
        public int Id { get; set; }

        [Column(Name = "service_id", StringLength = 100)]
        public string ServiceId { get; set; }

        [Column(Name = "service_name", StringLength = 100)]
        public string ServiceName { get; set; }

        [Column(Name = "ip", StringLength = 100)]
        public string Ip { get; set; }

        [Column(Name = "port")]
        public int? Port { get; set; }

        [Column(Name = "meta_data", StringLength = 2000)]
        public string MetaData { get; set; }
    }
}
