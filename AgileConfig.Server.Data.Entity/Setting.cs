using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "setting")]
    public class Setting
    {
        [Column(Name = "id", DbType = "varchar(36)")]
        public string Id { get; set; }

        [Column(Name = "value", DbType = "nvarchar(50)")]
        public string Value { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
