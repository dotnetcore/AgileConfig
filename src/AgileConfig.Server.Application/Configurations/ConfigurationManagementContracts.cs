using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application.Configurations;

public sealed record CreateConfigurationCommand(
    string Id,
    string ApplicationId,
    string Group,
    string Key,
    string Value,
    string Description,
    string Environment);

public sealed record UpdateConfigurationCommand(
    string Id,
    string ApplicationId,
    string Group,
    string Key,
    string Value,
    string Description,
    string Environment);

public sealed record DeleteConfigurationCommand(string Id, string Environment);

public interface IConfigurationManagementService
{
    Task<ApplicationResult<Config>> CreateAsync(CreateConfigurationCommand command);

    Task<ApplicationResult<Config>> UpdateAsync(UpdateConfigurationCommand command);

    Task<ApplicationResult<Config>> DeleteAsync(DeleteConfigurationCommand command);

    Task<IReadOnlyList<Config>> GetAllAsync(string applicationId, string environment);

    Task<Config> GetAsync(string applicationId, string environment, string configurationId);
}
