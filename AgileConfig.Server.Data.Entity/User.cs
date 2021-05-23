using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user")]
    public class User
    {
        [Column(Name= "user_name" , StringLength = 50, IsPrimary = true)]
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

        [Column(Name = "enabled")]
        public bool Enabled { get; set; }
    }
}
