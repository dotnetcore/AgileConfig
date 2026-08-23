using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application.Releases;

public sealed class ReleaseManagementService : IReleaseManagementService
{
    private readonly IAppService _appService;
    private readonly IConfigService _configService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITinyEventBus _eventBus;
    private readonly TimeProvider _timeProvider;

    public ReleaseManagementService(
        IAppService appService,
        IConfigService configService,
        ICurrentUserAccessor currentUserAccessor,
        ITinyEventBus eventBus,
        TimeProvider timeProvider)
    {
        _appService = appService;
        _configService = configService;
        _currentUserAccessor = currentUserAccessor;
        _eventBus = eventBus;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<PublishTimeline>> GetAllAsync(string applicationId, string environment)
    {
        var releases = await _configService.GetPublishTimelineHistoryAsync(applicationId, environment);
        return releases.OrderByDescending(x => x.Version).ToList();
    }

    public async Task<PublishTimeline> GetAsync(string applicationId, string environment, string releaseId)
    {
        var release = await _configService.GetPublishTimeLineNodeAsync(releaseId, environment);
        return release?.AppId == applicationId ? release : null;
    }

    public async Task<ApplicationResult<PublishTimeline>> PublishAsync(PublishConfigurationsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await _appService.GetAsync(command.ApplicationId) == null)
            return ApplicationResult<PublishTimeline>.Failure(ApplicationError.NotFound);

        var userId = await _currentUserAccessor.GetUserIdAsync();
        var result = await _configService.Publish(
            command.ApplicationId,
            command.ConfigurationIds,
            command.Log,
            userId,
            command.Environment);
        if (!result.result) return ApplicationResult<PublishTimeline>.Failure(ApplicationError.OperationFailed);

        var timeline = await _configService.GetPublishTimeLineNodeAsync(
            result.publishTimelineId,
            command.Environment);
        timeline ??= new PublishTimeline
        {
            Id = result.publishTimelineId,
            AppId = command.ApplicationId,
            Env = command.Environment,
            Log = command.Log,
            PublishUserId = userId,
            PublishTime = _timeProvider.GetLocalNow().DateTime
        };

        _eventBus.Fire(new PublishConfigSuccessful(timeline, _currentUserAccessor.UserName));
        return ApplicationResult<PublishTimeline>.Success(timeline);
    }

    public async Task<ApplicationResult<PublishTimeline>> RollbackAsync(RollbackConfigurationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var timeline = await _configService.GetPublishTimeLineNodeAsync(command.ReleaseId, command.Environment);
        if (timeline == null) return ApplicationResult<PublishTimeline>.Failure(ApplicationError.NotFound);

        var result = await _configService.RollbackAsync(command.ReleaseId, command.Environment);
        if (!result) return ApplicationResult<PublishTimeline>.Failure(ApplicationError.OperationFailed);

        _eventBus.Fire(new RollbackConfigSuccessful(timeline, _currentUserAccessor.UserName));
        return ApplicationResult<PublishTimeline>.Success(timeline);
    }
}
