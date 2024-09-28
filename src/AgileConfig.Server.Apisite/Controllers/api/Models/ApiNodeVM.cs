using AgileConfig.Server.Data.Entity;
using System;
using AgileConfig.Server.Apisite.Models;

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

    public static class ApiNodeVMExtension
    {
        public static ServerNodeVM ToServerNodeVM(this ApiNodeVM node)
        {
            if (node == null)
            {
                return null;
            }

            var vm = new ServerNodeVM
            {
                Address = node.Address,
                Remark = node.Remark,
                LastEchoTime = node.LastEchoTime,
                Status = node.Status
            };

            return vm;
        }
    }
}
