using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiPublishTimelineVM
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 应用id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 发布者
        /// </summary>
        public string PublishUserId { get; set; }

        /// <summary>
        /// 发布版本序号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 发布日志
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// 环境
        /// </summary>
        public string Env { get; set; }
    }
}
