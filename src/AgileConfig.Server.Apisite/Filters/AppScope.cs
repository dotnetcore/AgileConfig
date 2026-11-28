namespace AgileConfig.Server.Apisite.Filters;

/// <summary>
///     Tells <see cref="PermissionCheckAttribute" /> which application(s) an action operates on, so that
///     non-administrators are limited to the applications they created or were authorized for.
/// </summary>
public enum AppScope
{
    /// <summary>
    ///     The action is not bound to an application.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The action limits itself using <see cref="AppAccessHttpContextExtensions.GetUserAppAccess" />.
    /// </summary>
    InAction,

    /// <summary>
    ///     The application is identified by an "appId", "applicationId" or "id" argument, or by a model carrying it.
    /// </summary>
    App,

    /// <summary>
    ///     The "id", "configId" or "ids" arguments are configuration ids; their applications are looked up.
    /// </summary>
    Config,

    /// <summary>
    ///     The "publishTimelineId" or "historyId" argument is a publish record; its application is looked up.
    /// </summary>
    PublishTimeline
}
