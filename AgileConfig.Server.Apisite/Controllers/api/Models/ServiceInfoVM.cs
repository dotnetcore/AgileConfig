using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class ServiceInfoVM
    {
        public string ServiceId { get; set; } = "";

        public string ServiceName { get; set; } = "";

        public string Ip { get; set; } = "";

        public int? Port { get; set; }

        public List<string> MetaData { get; set; } = new List<string>();

        public ServiceStatus Status { get; set; }
    }
    
    public class QueryServiceInfoResultVM
    {
        public List<ServiceInfoVM> Data { get; set; }
    }
}
