using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Common;

public interface IEnvAccessor
{
    string Env { get; }
}

public class EnvAccessor : IEnvAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EnvAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Env
    {
        get
        {
            var env = _httpContextAccessor.HttpContext?.Request.Query["env"].FirstOrDefault();
            return env;
        }
    }
}

public static class EnvAccessorServiceCollectionExtension
{
    public static IServiceCollection AddEnvAccessor(this IServiceCollection services)
    {
        services.AddSingleton<IEnvAccessor, EnvAccessor>();

        return services;
    }
}