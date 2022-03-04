﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum ServiceAlive
    {
        Offline = 0,
        Online = 1
    }

    [Table(Name = "agc_service_info")]
    [OraclePrimaryKeyName("agc_serviceinfo_pk")]
    public class ServiceInfo
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

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

        [Column(Name = "alive")]
        public ServiceAlive Alive { get; set; }

        [Column(Name = "register_time")]

        public DateTime? RegisterTime { get; set; }

        [Column(Name = "last_heart_beat")]

        public DateTime? LastHeartBeat { get; set; }

        [Column(Name = "heart_beat_mode")]
        public string HeartBeatMode { get; set; }

        [Column(Name = "check_url", StringLength = 2000)]
        public string CheckUrl { get; set; }
    }
  
}
