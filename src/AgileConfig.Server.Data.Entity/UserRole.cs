using FreeSql.DataAnnotations;
using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_role")]
    [OraclePrimaryKeyName("agc_user_role_pk")]
    public class UserRole: IEntity<string>
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "user_id", StringLength = 50)]
        public string UserId { get; set; }

        [Column(Name = "role")]
        public Role Role { get; set; }

        [Column(Name = "create_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

    }

    public enum Role
    {
        [Description("Super Administrator")]
        SuperAdmin = 0,
        [Description("Administrator")]
        Admin = 1,
        [Description("Operator")]
        NormalUser = 2, 
    }
}
