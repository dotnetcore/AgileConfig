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

    public const string Confing_Read = "CONFIG_READ";
    public const string Config_Add = "CONFIG_ADD";
    public const string Config_Edit = "CONFIG_EDIT";
    public const string Config_Delete = "CONFIG_DELETE";

    public const string Config_Publish = "CONFIG_PUBLISH";
    public const string Config_Offline = "CONFIG_OFFLINE";

    public const string Node_Read = "NODE_READ";
    public const string Node_Add = "NODE_ADD";
    public const string Node_Delete = "NODE_DELETE";

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