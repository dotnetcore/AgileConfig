using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class PublishLogVM: IAppIdModel
    {
        public string AppId { get; set; }
        public string Log { get; set; }
    }
}
