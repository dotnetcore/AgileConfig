using System;
using System.Collections.Generic;

namespace AgileConfig.Server.IService;

/// <summary>
///     The applications a user is allowed to operate on. Administrators may operate on every application;
///     other users only on the applications they created or were authorized for.
/// </summary>
public sealed class UserAppAccess
{
    /// <summary>
    ///     Access that allows no application at all.
    /// </summary>
    public static readonly UserAppAccess None = new(null, false, []);

    // Ordinal on purpose: matching more loosely than the database would grant access to a different app.
    private readonly HashSet<string> _appIds;

    private UserAppAccess(string userId, bool isAdmin, IEnumerable<string> appIds)
    {
        UserId = userId;
        IsAdmin = isAdmin;
        _appIds = new HashSet<string>(appIds, StringComparer.Ordinal);
    }

    public string UserId { get; }

    public bool IsAdmin { get; }

    public static UserAppAccess Admin(string userId)
    {
        return new UserAppAccess(userId, true, []);
    }

    public static UserAppAccess Restricted(string userId, IEnumerable<string> appIds)
    {
        return new UserAppAccess(userId, false, appIds);
    }

    public bool CanAccess(string appId)
    {
        return IsAdmin || (!string.IsNullOrEmpty(appId) && _appIds.Contains(appId));
    }
}
