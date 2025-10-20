using FreeSql.DataAnnotations;
using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_role")]
    [OraclePrimaryKeyName("agc_user_role_pk")]
    public class UserRole : IEntity<string>
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "user_id", StringLength = 50)]
        public string UserId { get; set; }

        [Column(Name = "role_id", StringLength = 64)]
        public string RoleId { get; set; }

        [Column(Name = "role")]
        public int? LegacyRoleValue { get; set; }

        [Column(Name = "create_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
    }
}
