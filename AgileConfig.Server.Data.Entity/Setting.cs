using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_setting")]
    [OraclePrimaryKeyName("agc_setting_pk")]
    public class Setting
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "value", StringLength = 200)]
        public string Value { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
