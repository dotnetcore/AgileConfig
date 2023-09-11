using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiConfigVM : IAppIdModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 应用
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 组
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 状态 
        /// </summary>
        public ConfigStatus Status { get; set; }

        /// <summary>
        /// 在线状态
        /// </summary>
        public OnlineStatus OnlineStatus { get; set; }

        /// <summary>
        /// 编辑状态
        /// </summary>
        public EditStatus EditStatus { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
