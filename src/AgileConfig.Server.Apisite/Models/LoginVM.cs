using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models;

[ExcludeFromCodeCoverage]
public class LoginVM
{
    public string userName { get; set; }
    public string password { get; set; }
}