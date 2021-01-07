using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_modifylog")]
    public class ModifyLog
    {
        [Column(Name = "id", StringLength=36)]
        public string Id { get; set; }

        [Column(Name = "config_id", StringLength = 36)]
        public string ConfigId { get; set; }

        [Column(Name = "g", StringLength = 100)]
        public string Group { get; set; }

        [Column(Name = "k", StringLength = 100)]
        public string Key { get; set; }

        public string Value { get; set; }

        [Column(Name = "modify_time")]
        public DateTime ModifyTime { get; set; }
    }
}
