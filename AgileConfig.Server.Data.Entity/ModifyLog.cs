using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table("modifylog")]
    public class ModifyLog
    {
        [Key]
        [Column("id", TypeName = "varchar(36)")]
        public string Id { get; set; }

        [Column("config_id", TypeName = "varchar(36)")]
        public string ConfigId { get; set; }

        [Column("g", TypeName = "nvarchar(100)")]
        public string Group { get; set; }

        [Column("k", TypeName = "nvarchar(100)")]
        public string Key { get; set; }

        [Column("v", TypeName = "nvarchar(4000)")]
        public string Value { get; set; }

        [Column("modify_time")]
        public DateTime ModifyTime { get; set; }
    }
}
