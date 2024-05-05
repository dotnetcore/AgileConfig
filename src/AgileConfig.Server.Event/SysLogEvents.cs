using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using System.Xml.Linq;

namespace AgileConfig.Server.Event
{
    public class LoginEvent : IEvent
    {
        public LoginEvent(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }

    public class InitSaPasswordSuccessful : IEvent
    {
    }

    public class ResetUserPasswordSuccessful : IEvent
    {
        public ResetUserPasswordSuccessful(string opUser, string userName)
        {
            OpUser = opUser;
            UserName = userName;
        }

        public string OpUser { get; }
        public string UserName { get; }
    }

    public class ChangeUserPasswordSuccessful : IEvent
    {
        public ChangeUserPasswordSuccessful(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }

    public class AddAppSuccessful : IEvent
    {
        public AddAppSuccessful(App app, string userName)
        {
            App = app;
            UserName = userName;
        }

        public App App { get; }
        public string UserName { get; }
    }

    public class EditAppSuccessful : IEvent
    {
        public EditAppSuccessful(App app, string userName)
        {
            App = app;
            UserName = userName;
        }

        public App App { get; }
        public string UserName { get; }
    }

    public class DisableOrEnableAppSuccessful : IEvent
    {
        public DisableOrEnableAppSuccessful(App app, string userName)
        {
            App = app;
            UserName = userName;
        }

        public App App { get; }
        public string UserName { get; }
    }

    public class DeleteAppSuccessful : IEvent
    {
        public DeleteAppSuccessful(App app, string userName)
        {
            App = app;
            UserName = userName;
        }

        public App App { get; }
        public string UserName { get; }
    }

    public class AddConfigSuccessful : IEvent
    {
        public AddConfigSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }
    }

    public class EditConfigSuccessful : IEvent
    {
        public EditConfigSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }
    }

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

    public class DeleteSomeConfigSuccessful : IEvent
    {
        public DeleteSomeConfigSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }
    }

    public class PublishConfigSuccessful : IEvent
    {
        public PublishConfigSuccessful( PublishTimeline publishTimeline, string userName)
        {
            PublishTimeline = publishTimeline;
            UserName = userName;
        }

        public PublishTimeline PublishTimeline { get; }
        public string UserName { get; }
    }

    public class RollbackConfigSuccessful : IEvent
    {
        public RollbackConfigSuccessful(PublishTimeline timelineNode, string userName)
        {
            TimelineNode = timelineNode;
            UserName = userName;
        }

        public PublishTimeline TimelineNode { get; }
        public string UserName { get; }
    }

    public class CancelEditConfigSuccessful : IEvent
    {
        public CancelEditConfigSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }
    }

    public class CancelEditConfigSomeSuccessful : IEvent
    {
        public CancelEditConfigSomeSuccessful(Config config, string userName)
        {
            Config = config;
            UserName = userName;
        }

        public Config Config { get; }
        public string UserName { get; }

        public string Env { get; set; }
    }

    public class AddNodeSuccessful : IEvent
    {
        public AddNodeSuccessful(ServerNode node, string userName)
        {
            Node = node;
            UserName = userName;
        }

        public ServerNode Node { get; }
        public string UserName { get; }
    }

    public class DeleteNodeSuccessful : IEvent
    {
        public DeleteNodeSuccessful(ServerNode node, string userName)
        {
            Node = node;
            UserName = userName;
        }

        public ServerNode Node { get; }
        public string UserName { get; }
    }

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

    public class DeleteUserSuccessful : IEvent
    {
        public DeleteUserSuccessful(User user, string userName)
        {
            UserName = userName;
            User = user;
        }

        public string UserName { get; }
        public User User { get; }
    }

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

    public class RegisterAServiceSuccessful : IEvent
    {
        public RegisterAServiceSuccessful(string serviceId, string serviceName, string userName)
        {
            ServiceId = serviceId;
            ServiceName = serviceName;
            UserName = userName;
        }

        public string ServiceId { get; }
        public string ServiceName { get; }
        public string UserName { get; }
    }

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
