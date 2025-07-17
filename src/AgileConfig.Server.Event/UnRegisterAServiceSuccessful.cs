using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event
{

    public class UnRegisterAServiceSuccessful : IEvent
    {
        public UnRegisterAServiceSuccessful(string serviceId, string serviceName, string userName)
        {
            ServiceId = serviceId;
            ServiceName = serviceName;
            UserName = userName;
        }

        public string ServiceId { get; }
        public string ServiceName { get; }
        public string UserName { get; }
    }
}
