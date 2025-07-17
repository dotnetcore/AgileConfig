using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event
{
    public class DiscoinnectSuccessful : IEvent
    {
        public DiscoinnectSuccessful(string clientId, string userName)
        {
            ClientId = clientId;
            UserName = userName;
        }

        public string ClientId { get; }
        public string UserName { get; }
    }
}
