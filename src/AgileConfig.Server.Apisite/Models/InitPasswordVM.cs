using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models;

[ExcludeFromCodeCoverage]
public class InitPasswordVM
{
    public string password { get; set; }

    public string confirmPassword { get; set; }
}