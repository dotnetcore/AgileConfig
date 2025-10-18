using AgileConfig.Server.Data.Entity;
using System;
using AgileConfig.Server.Apisite.Models;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiNodeVM
    {
        /// <summary>
        /// Node address.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Remarks.
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// Node status.
        /// </summary>
        public NodeStatus Status { get; set; }
        /// <summary>
        /// Time of the last echo response.
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
