using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgileConfig.Server.Data.Entity
{
    [Table("config")]
    public class Config
    {
        [Key]
        [Column("id")]
        public string Id { get; set; }

        [Column("app_id")]
        public string AppId { get; set; }

        [Column("k")]
        public string Key { get; set; }

        [Column("v")]
        public string Value { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }
    }
}
