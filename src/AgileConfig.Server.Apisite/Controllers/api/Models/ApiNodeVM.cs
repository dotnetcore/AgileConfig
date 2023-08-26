using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiNodeVM
    {
        /// <summary>
        /// 节点地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public NodeStatus Status { get; set; }
        /// <summary>
        /// 最后响应时间
        /// </summary>
        public DateTime? LastEchoTime { get; set; }
    }
}
