using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Event
{
    public class EditUserSuccessful : IEvent
    {
        public EditUserSuccessful(User user, string userName)
        {
            User = user;
            UserName = userName;
        }

        public User User { get; }
        public string UserName { get; }
    }
}
