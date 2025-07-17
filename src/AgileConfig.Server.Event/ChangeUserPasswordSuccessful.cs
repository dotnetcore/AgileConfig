using AgileConfig.Server.Common.EventBus;

namespace AgileConfig.Server.Event
{
    public class ChangeUserPasswordSuccessful : IEvent
    {
        public ChangeUserPasswordSuccessful(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }
}
