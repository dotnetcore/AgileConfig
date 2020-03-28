using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table("setting")]
    public class Setting
    {
        [Key]
        [Column("id", TypeName = "varchar(36)")]
        public string Id { get; set; }

        [Column("value", TypeName = "nvarchar(50)")]
        public string Value { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
