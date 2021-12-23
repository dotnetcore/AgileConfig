using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user")]
    [OraclePrimaryKeyName("agc_user_pk")]
    public class User
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name= "user_name" , StringLength = 50)]
        public string UserName { get; set; }

        [Column(Name = "password", StringLength = 50)]
        public string Password { get; set; }

        [Column(Name = "salt", StringLength = 36)]
        public string Salt { get; set; }

        [Column(Name = "team", StringLength = 50)]
        public string Team { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "status")]
        public UserStatus Status { get; set; }
    }

    public enum UserStatus
    {
        Normal  = 0,
        Deleted = -1
    }
}
