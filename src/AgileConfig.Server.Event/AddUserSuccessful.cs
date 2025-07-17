using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event
{
    public class AddUserSuccessful : IEvent
    {
        public AddUserSuccessful(User user, string userName)
        {
            User = user;
            UserName = userName;
        }

        public User User { get; }
        public string UserName { get; }
    }
}
