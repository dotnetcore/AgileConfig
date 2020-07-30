using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class ServerNodeVM
    {
        [Required(ErrorMessage = "节点地址不能为空")]
        [MaxLength(100, ErrorMessage = "节点地址长度不能超过100位")]
        public string Address { get; set; }

        [MaxLength(50, ErrorMessage = "备注长度不能超过50位")]
        public string Remark { get; set; }
        public NodeStatus Status { get; set; }
        public DateTime? LastEchoTime { get; set; }
    }
}
