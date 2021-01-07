using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    public enum ConfigStatus
    {
        Deleted = 0,
        Enabled = 1,
    }

    public enum OnlineStatus
    {
        WaitPublish = 0,
        Online = 1,
    }

    [Table(Name = "agc_config")]
    public class Config
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "app_id", StringLength = 36)]
        public string AppId { get; set; }

        [Column(Name = "g", StringLength = 100)]
        public string Group { get; set; }

        [Column(Name = "k", StringLength = 100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [Column(Name = "description", StringLength = 200)]
        public string Description { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "status")]
        public ConfigStatus Status { get; set; }

        [Column(Name = "online_status")]
        public OnlineStatus OnlineStatus { get; set; }
    }
}
