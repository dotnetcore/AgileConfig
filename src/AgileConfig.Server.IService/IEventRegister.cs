using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.IService
{
    public interface IEventRegister
    {
        void Register();
    }

    public class EventKeys
    {
        public const string USER_LOGIN_SUCCESS = "USER_LOGIN_SUCCESS";

        public const string INIT_SUPERADMIN_PASSWORD_SUCCESS = "INIT_SUPERADMIN_PASSWORD_SUCCESS";

        public const string ADD_APP_SUCCESS = "ADD_APP_SUCCESS";
        public const string EDIT_APP_SUCCESS = "EDIT_APP_SUCCESS";
        public const string DISABLE_OR_ENABLE_APP_SUCCESS = "DISABLE_OR_ENABLE_APP_SUCCESS";
        public const string DELETE_APP_SUCCESS = "DELETE_APP_SUCCESS";

        public const string ADD_CONFIG_SUCCESS = "ADD_CONFIG_SUCCESS";
        public const string EDIT_CONFIG_SUCCESS = "EDIT_CONFIG_SUCCESS";
        public const string DELETE_CONFIG_SUCCESS = "DELETE_CONFIG_SUCCESS";
        public const string DELETE_CONFIG_SOME_SUCCESS = "DELETE_CONFIG_SOME_SUCCESS";

        public const string PUBLISH_CONFIG_SUCCESS = "PUBLISH_CONFIG_SUCCESS";
        public const string OFFLINE_CONFIG_SUCCESS = "OFFLINE_CONFIG_SUCCESS";
        public const string ROLLBACK_CONFIG_SUCCESS = "ROLLBACK_CONFIG_SUCCESS";
        public const string CANCEL_EDIT_CONFIG_SUCCESS = "CANCEL_EDIT_CONFIG_SUCCESS";
        public const string CANCEL_EDIT_CONFIG_SOME_SUCCESS = "CANCEL_EDIT_CONFIG_SOME_SUCCESS";

        public const string ADD_NODE_SUCCESS = "ADD_NODE_SUCCESS";
        public const string DELETE_NODE_SUCCESS = "DELETE_NODE_SUCCESS";

        public const string ADD_USER_SUCCESS = "ADD_USER_SUCCESS";
        public const string EDIT_USER_SUCCESS = "EDIT_USER_SUCCESS";
        public const string DELETE_USER_SUCCESS = "DELETE_USER_SUCCESS";
        public const string CHANGE_USER_PASSWORD_SUCCESS = "CHANGE_USER_PASSWORD_SUCCESS";
        public const string RESET_USER_PASSWORD_SUCCESS = "RESET_USER_PASSWORD_SUCCESS";

        public const string DISCONNECT_CLIENT_SUCCESS = "DISCONNECT_CLIENT_SUCCESS";
        
        public const string REGISTER_A_SERVICE = "REGISTER_A_SERVICE";
        public const string UNREGISTER_A_SERVICE = "UNREGISTER_A_SERVICE";
        public const string UPDATE_SERVICE_STATUS = "UPDATE_SERVICE_STATUS";

    }
}
