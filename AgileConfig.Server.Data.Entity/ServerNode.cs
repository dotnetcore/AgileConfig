using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum NodeStatus
    {
        Online = 1,
        Offline = 0,
    }

    [Table("server_node")]
    public class ServerNode
    {
        [Key]
        [Column("address", TypeName = "nvarchar(50)")]
        public string Address { get; set; }

        [Column("remark", TypeName = "nvarchar(50)")]
        public string Remark { get; set; }

        [Column("status")]
        public NodeStatus Status { get; set; }

        [Column("last_echo_time")]
        public DateTime? LastEchoTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
