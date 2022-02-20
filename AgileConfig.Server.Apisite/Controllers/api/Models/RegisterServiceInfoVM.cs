using System.Collections.Generic;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class RegisterServiceInfoVM
    {
        public string ServiceId { get; set; } = "";

        public string ServiceName { get; set; } = "";

        public string Ip { get; set; } = "";

        public int Port { get; set; }

        public List<string> MetaData { get; set; } = new List<string>();

    }
}
