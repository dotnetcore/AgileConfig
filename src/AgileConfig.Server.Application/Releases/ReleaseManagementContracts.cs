using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Application.Releases;

public sealed record PublishConfigurationsCommand(
    string ApplicationId,
    string[] ConfigurationIds,
    string Log,
    string Environment);

public sealed record RollbackConfigurationCommand(string ReleaseId, string Environment);

public interface IReleaseManagementService
{
    Task<IReadOnlyList<PublishTimeline>> GetAllAsync(string applicationId, string environment);

    Task<PublishTimeline> GetAsync(string applicationId, string environment, string releaseId);

    Task<ApplicationResult<PublishTimeline>> PublishAsync(PublishConfigurationsCommand command);

    Task<ApplicationResult<PublishTimeline>> RollbackAsync(RollbackConfigurationCommand command);
}
