using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table("app")]
    public class App
    {
        [Key]
        [Column("id" ,TypeName ="nvarchar(8)")]
        public string Id { get; set; }

        [Column("name" , TypeName ="nvarchar(50)")]
        public string Name { get; set; }

        [Column("secret", TypeName = "nvarchar(36)")]
        public string Secret { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; }
    }
}
