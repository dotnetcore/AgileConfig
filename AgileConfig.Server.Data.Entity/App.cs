using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "app")]
    public class App
    {
        [Column(Name= "id" , DbType = "nvarchar(36)")]
        public string Id { get; set; }

        [Column(Name = "name" , DbType = "nvarchar(50)")]
        public string Name { get; set; }

        [Column(Name = "secret", DbType = "nvarchar(36)")]
        public string Secret { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "enabled")]
        public bool Enabled { get; set; }
    }
}
