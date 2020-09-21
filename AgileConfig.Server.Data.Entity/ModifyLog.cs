using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "modifylog")]
    public class ModifyLog
    {
        [Column(Name = "id", DbType = "varchar(36)")]
        public string Id { get; set; }

        [Column(Name = "config_id", DbType = "varchar(36)")]
        public string ConfigId { get; set; }

        [Column(Name = "g", DbType = "nvarchar(100)")]
        public string Group { get; set; }

        [Column(Name = "k", DbType = "nvarchar(100)")]
        public string Key { get; set; }

        [Column(Name = "v", DbType = "nvarchar(4000)")]
        public string Value { get; set; }

        [Column(Name = "modify_time")]
        public DateTime ModifyTime { get; set; }
    }
}
