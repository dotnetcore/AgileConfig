using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgileConfig.Server.IService;

public static class Functions
{
    public const string App_Read = "APP_READ";
    public const string App_Add = "APP_ADD";
    public const string App_Edit = "APP_EDIT";
    public const string App_Delete = "APP_DELETE";
    public const string App_Auth = "APP_AUTH";

    public const string Config_Read = "CONFIG_READ";
    public const string Config_Add = "CONFIG_ADD";
    public const string Config_Edit = "CONFIG_EDIT";
    public const string Config_Delete = "CONFIG_DELETE";

    public const string Config_Publish = "CONFIG_PUBLISH";
    public const string Config_Offline = "CONFIG_OFFLINE";

    public const string Node_Read = "NODE_READ";
    public const string Node_Add = "NODE_ADD";
    public const string Node_Delete = "NODE_DELETE";

    public const string Client_Read = "CLIENT_READ";
    public const string Client_Refresh = "CLIENT_REFRESH";
    public const string Client_Disconnect = "CLIENT_DISCONNECT";

    public const string User_Read = "USER_READ";
    public const string User_Add = "USER_ADD";
    public const string User_Edit = "USER_EDIT";
    public const string User_Delete = "USER_DELETE";

    public const string Role_Read = "ROLE_READ";
    public const string Role_Add = "ROLE_ADD";
    public const string Role_Edit = "ROLE_EDIT";
    public const string Role_Delete = "ROLE_DELETE";

    public const string Service_Read = "SERVICE_READ";
    public const string Service_Add = "SERVICE_ADD";
    public const string Service_Delete = "SERVICE_DELETE";

    public const string Log_Read = "LOG_READ";

    public static List<string> GetAllPermissions()
    {
        // SuperAdmin has all permissions
        return new List<string>
        {
            // Application permissions
            Functions.App_Read,
            Functions.App_Add,
            Functions.App_Edit,
            Functions.App_Delete,
            Functions.App_Auth,

            // Configuration permissions
            Functions.Config_Read,
            Functions.Config_Add,
            Functions.Config_Edit,
            Functions.Config_Delete,
            Functions.Config_Publish,
            Functions.Config_Offline,

            // Node permissions
            Functions.Node_Read,
            Functions.Node_Add,
            Functions.Node_Delete,

            // Client permissions
            Functions.Client_Refresh,
            Functions.Client_Disconnect,
            Functions.Client_Read,

            // User permissions
            Functions.User_Read,
            Functions.User_Add,
            Functions.User_Edit,
            Functions.User_Delete,

            // Role permissions
            Functions.Role_Read,
            Functions.Role_Add,
            Functions.Role_Edit,
            Functions.Role_Delete,

            // Service permissions
            Functions.Service_Read,
            Functions.Service_Add,
            Functions.Service_Delete,

            // System permissions
            Functions.Log_Read
        };
    }

}

public interface IPermissionService
{
    string EditConfigPermissionKey { get; }

    string PublishConfigPermissionKey { get; }

    Task<List<string>> GetUserPermission(string userId);

    /// <summary>
    ///     Retrieve the categories of permissions granted to a user.
    /// </summary>
    /// <param name="userId">Identifier of the user requesting categories.</param>
    /// <returns>List of distinct categories granted to the user.</returns>
    Task<List<string>> GetUserCategories(string userId);
}