using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application.Configurations;

public interface IPublishedConfigurationQueryService
{
    Task<ApplicationResult<PublishedConfigurationSnapshot>> GetAsync(
        string applicationId,
        string environment);
}

public sealed record PublishedConfigurationSnapshot(
    string PublishTimelineId,
    IReadOnlyList<Config> Configurations);

public sealed class PublishedConfigurationQueryService : IPublishedConfigurationQueryService
{
    private readonly IAppService _appService;
    private readonly IConfigService _configService;

    public PublishedConfigurationQueryService(IAppService appService, IConfigService configService)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    public async Task<ApplicationResult<PublishedConfigurationSnapshot>> GetAsync(
        string applicationId,
        string environment)
    {
        var application = await _appService.GetAsync(applicationId);
        if (application == null || !application.Enabled)
            return ApplicationResult<PublishedConfigurationSnapshot>.Failure(ApplicationError.NotFound);

        var timelineId = await _configService.GetLastPublishTimelineVirtualIdAsyncWithCache(
            applicationId,
            environment);
        var configurations = await _configService.GetPublishedConfigsByAppIdWithInheritance(
            applicationId,
            environment);

        return ApplicationResult<PublishedConfigurationSnapshot>.Success(
            new PublishedConfigurationSnapshot(timelineId, configurations));
    }
}
