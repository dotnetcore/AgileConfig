using FreeSql.DataAnnotations;
using System;
using MongoDB.Bson.Serialization.Attributes;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_setting")]
    [OraclePrimaryKeyName("agc_setting_pk")]
    public class Setting: IEntity<string>
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "value", StringLength = 200)]
        public string Value { get; set; }

        [Column(Name = "create_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
    }
}
