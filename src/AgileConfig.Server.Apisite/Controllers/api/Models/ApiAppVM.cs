using System;
using System.Collections.Generic;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers.api.Models;

/// <summary>
///     Application model returned by the REST API.
/// </summary>
public class ApiAppVM : IAppModel
{
    /// <summary>
    ///     Whether the application can inherit configuration.
    /// </summary>
    public bool Inheritanced { get; set; }

    /// <summary>
    ///     Application secret.
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    ///     Whether the application is enabled.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    ///     Applications inherited by this application.
    /// </summary>
    public List<string> InheritancedApps { get; set; }

    /// <summary>
    ///     id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     name
    /// </summary>
    public string Name { get; set; }

    public string Group { get; set; }

    public string Creator { get; set; }

    public DateTime CreateTime { get; set; }
}

public static class ApiAppVMExtension
{
    public static AppVM ToAppVM(this ApiAppVM vm)
    {
        if (vm == null) return null;

        return new AppVM
        {
            Id = vm.Id,
            Name = vm.Name,
            Secret = vm.Secret,
            Inheritanced = vm.Inheritanced,
            Creator = vm.Creator,
            Group = vm.Group,
            Enabled = vm.Enabled.GetValueOrDefault()
        };
    }
}
