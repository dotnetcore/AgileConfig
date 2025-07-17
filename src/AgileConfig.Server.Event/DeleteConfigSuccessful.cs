using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event
{
    public class DeleteConfigSuccessful : IEvent
    {
        public DeleteConfigSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }
    }
}
