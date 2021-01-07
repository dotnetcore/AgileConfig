using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum AppType
    {
        PRIVATE,
        Inheritance,
    }

    [Table(Name = "agc_app")]
    public class App
    {
        [Column(Name= "id" , StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "name" , StringLength = 50)]
        public string Name { get; set; }

        [Column(Name = "secret", StringLength = 36)]
        public string Secret { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "enabled")]
        public bool Enabled { get; set; }

        [Column(Name = "type")]
        public AppType Type { get; set; }
    }
}
