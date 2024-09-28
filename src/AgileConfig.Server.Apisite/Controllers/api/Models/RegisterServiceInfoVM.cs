using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    public class RegisterServiceInfoVM
    {
        public string ServiceId { get; set; } = "";

        public string ServiceName { get; set; } = "";

        public string Ip { get; set; } = "";

        public int? Port { get; set; }

        public List<string> MetaData { get; set; } = new List<string>();

        public string CheckUrl { get; set; } = "";
        
        public string AlarmUrl { get; set; } = "";

        public string HeartBeatMode { get; set; }
    }

    public static class RegisterServiceInfoVMExtension
    {
        public static ServiceInfo ToServiceInfo(this RegisterServiceInfoVM model)
        {
            if (model == null)
            {
                return null;
            }

            return new ServiceInfo
            {
                ServiceId = model.ServiceId,
                ServiceName = model.ServiceName,
                Ip = model.Ip,
                Port = model.Port,
                MetaData = model.MetaData is null ? "[]" : JsonConvert.SerializeObject(model.MetaData),
                CheckUrl = model.CheckUrl,
                AlarmUrl = model.AlarmUrl,
                HeartBeatMode = model.HeartBeatMode
            };
        }
    }
}