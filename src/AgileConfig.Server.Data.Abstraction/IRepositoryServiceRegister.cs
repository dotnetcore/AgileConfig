using Microsoft.Extensions.DependencyInjection;

namespace AgileConfig.Server.Data.Abstraction;

/// <summary>
///     Defines the contract for integrating a new storage provider.
/// </summary>
public interface IRepositoryServiceRegister
{
    /// <summary>
    ///     Determine whether this register matches the specified provider name.
    /// </summary>
    /// <param name="provider">Database provider identifier.</param>
    /// <returns>True when the register supports the provider.</returns>
    bool IsSuit4Provider(string provider);

    /// <summary>
    ///     Register storage services that do not depend on an environment.
    /// </summary>
    /// <param name="sc">Service collection to populate.</param>
    void AddFixedRepositories(IServiceCollection sc);

    /// <summary>
    ///     Resolve an environment-specific storage service.
    /// </summary>
    /// <typeparam name="T">Type of repository to resolve.</typeparam>
    /// <param name="sp">Service provider used for resolution.</param>
    /// <param name="env">Environment identifier.</param>
    /// <returns>Repository instance for the environment.</returns>
    T GetServiceByEnv<T>(IServiceProvider sp, string env) where T : class;
}