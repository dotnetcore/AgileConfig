using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AgileConfig.Server.Common
{
    public interface IEnvAccessor
    {
        string Env { get; }
    }

    public class EnvAccessor : IEnvAccessor
    {
        public EnvAccessor(IHttpContextAccessor httpContextAccessor)
        {
            var env = httpContextAccessor.HttpContext.Request.Query["env"].FirstOrDefault();
            if (string.IsNullOrEmpty(env))
            {
                env = "DEV";
            }
            Env = env;
        }
        public string Env { get; }
    }

    public static class EnvAccessorServiceCollectionExtension
    {
        public static IServiceCollection AddEnvAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IEnvAccessor, EnvAccessor>();

            return services;
        }
    }
}
