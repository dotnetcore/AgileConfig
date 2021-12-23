using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_publish_detail")]
    [OraclePrimaryKeyName("agc_publish_detail_pk")]
    public class PublishDetail
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "app_id", StringLength = 36)]
        public string AppId { get; set; }

        public int Version { get; set; }

        [Column(Name = "publish_timeline_id", StringLength = 36)]
        public string PublishTimelineId { get; set; }

        [Column(Name = "config_id", StringLength = 36)]
        public string ConfigId { get; set; }

        [Column(Name = "g", StringLength = 100)]
        public string Group { get; set; }

        [Column(Name = "k", StringLength = 100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [Column(Name = "description", StringLength = 200)]
        public string Description { get; set; }

        [Column(Name = "edit_status")]
        public EditStatus EditStatus { get; set; }

        [Column(Name = "env", StringLength = 50)]
        public string Env { get; set; }
    }
}
