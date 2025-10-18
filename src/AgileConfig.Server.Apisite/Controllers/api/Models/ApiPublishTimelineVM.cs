using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ApiPublishTimelineVM
    {
        /// <summary>
        /// Publish record identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Application ID.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Publish time.
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// Publisher.
        /// </summary>
        public string PublishUserId { get; set; }

        /// <summary>
        /// Publish version number.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Publish log text.
        /// </summary>
        public string Log { get; set; }

        /// <summary>
        /// Environment identifier.
        /// </summary>
        public string Env { get; set; }
    }
}
