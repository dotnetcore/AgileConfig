using AgileConfig.Server.Apisite.Models.Binders;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    [ModelBinder(BinderType = typeof(EnvQueryStringBinder))]
    public class EnvString 
    {
        public string Value { get; set; }
    }
}
