using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_publish_timeline")]
    [OraclePrimaryKeyName("agc_publish_timeline_pk")]
    public class PublishTimeline
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "app_id", StringLength = 36)]
        public string AppId { get; set; }

        [Column(Name = "publish_time")]
        public DateTime? PublishTime { get; set; }

        [Column(Name = "publish_user_id", StringLength = 36)]
        public string PublishUserId { get; set; }

        [Column(Name = "publish_user_name", StringLength = 50)]
        public string PublishUserName { get; set; }

        [Column(Name = "version")]
        public int Version { get; set; }

        [Column(Name = "log", StringLength = 100)]
        public string Log { get; set; }

        [Column(Name = "env", StringLength = 50)]
        public string Env { get; set; }
    }
}
