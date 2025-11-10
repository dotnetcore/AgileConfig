using System.Diagnostics.CodeAnalysis;
using AgileConfig.Server.Apisite.Models.Binders;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Models;

[ExcludeFromCodeCoverage]
[ModelBinder(BinderType = typeof(EnvQueryStringBinder))]
public class EnvString
{
    public string Value { get; set; }
}