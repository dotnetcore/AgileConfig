using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_publish_timeline")]
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

        public  int Version { get; set; }

    }
}
