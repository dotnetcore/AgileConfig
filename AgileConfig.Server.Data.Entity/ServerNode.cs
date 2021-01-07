using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    public enum NodeStatus
    {
        Online = 1,
        Offline = 0,
    }

    [Table(Name = "agc_server_node")]
    public class ServerNode
    {
        [Column(Name = "address", StringLength = 100, IsPrimary = true)]
        public string Address { get; set; }

        [Column(Name = "remark", StringLength = 50)]
        public string Remark { get; set; }

        [Column(Name = "status")]
        public NodeStatus Status { get; set; }

        [Column(Name = "last_echo_time")]
        public DateTime? LastEchoTime { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
