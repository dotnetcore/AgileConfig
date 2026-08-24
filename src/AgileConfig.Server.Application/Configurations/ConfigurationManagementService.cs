using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application.Configurations;

public sealed class ConfigurationManagementService : IConfigurationManagementService
{
    private readonly IAppService _appService;
    private readonly IConfigService _configService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITinyEventBus _eventBus;
    private readonly TimeProvider _timeProvider;

    public ConfigurationManagementService(
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

    public async Task<ApplicationResult<Config>> CreateAsync(CreateConfigurationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await _appService.GetAsync(command.ApplicationId) == null)
            return ApplicationResult<Config>.Failure(ApplicationError.NotFound);

        var existing = await _configService.GetByAppIdKeyEnv(
            command.ApplicationId,
            command.Group,
            command.Key,
            command.Environment);
        if (existing != null) return ApplicationResult<Config>.Failure(ApplicationError.Conflict);

        var config = new Config
        {
            Id = string.IsNullOrEmpty(command.Id) ? Guid.NewGuid().ToString("N") : command.Id,
            Key = command.Key,
            AppId = command.ApplicationId,
            Description = command.Description,
            Value = command.Value,
            Group = command.Group,
            Status = ConfigStatus.Enabled,
            CreateTime = _timeProvider.GetLocalNow().DateTime,
            UpdateTime = null,
            OnlineStatus = OnlineStatus.WaitPublish,
            EditStatus = EditStatus.Add,
            Env = command.Environment
        };

        var persisted = await _configService.AddAsync(config, command.Environment);
        if (!persisted) return ApplicationResult<Config>.Failure(ApplicationError.OperationFailed, config);

        _eventBus.Fire(new AddConfigSuccessful(config, _currentUserAccessor.UserName));
        return ApplicationResult<Config>.Success(config);
    }

    public async Task<ApplicationResult<Config>> UpdateAsync(UpdateConfigurationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var config = await _configService.GetAsync(command.Id, command.Environment);
        if (config == null) return ApplicationResult<Config>.Failure(ApplicationError.NotFound);

        // Preserve the legacy endpoint's application ownership check. The old API
        // treats an application with no editable configurations as nonexistent.
        var applicationConfigs = await _configService.GetByAppIdAsync(
            command.ApplicationId,
            command.Environment);
        if (!applicationConfigs.Any()) return ApplicationResult<Config>.Failure(ApplicationError.NotFound);

        var oldConfig = new Config
        {
            Key = config.Key,
            Group = config.Group,
            Value = config.Value
        };
        if (config.Group != command.Group || config.Key != command.Key)
        {
            var conflicting = await _configService.GetByAppIdKeyEnv(
                command.ApplicationId,
                command.Group,
                command.Key,
                command.Environment);
            if (conflicting != null) return ApplicationResult<Config>.Failure(ApplicationError.Conflict);
        }

        config.AppId = command.ApplicationId;
        config.Description = command.Description;
        config.Key = command.Key;
        config.Value = command.Value;
        config.Group = command.Group;
        config.UpdateTime = _timeProvider.GetLocalNow().DateTime;
        config.Env = command.Environment;

        if (!IsOnlyUpdateDescription(config, oldConfig))
        {
            var isPublished = await _configService.IsPublishedAsync(config.Id, command.Environment);
            config.EditStatus = isPublished ? EditStatus.Edit : EditStatus.Add;
            config.OnlineStatus = OnlineStatus.WaitPublish;
        }

        var persisted = await _configService.UpdateAsync(config, command.Environment);
        if (!persisted) return ApplicationResult<Config>.Failure(ApplicationError.OperationFailed);

        _eventBus.Fire(new EditConfigSuccessful(config, _currentUserAccessor.UserName));
        return ApplicationResult<Config>.Success(config);
    }

    public async Task<ApplicationResult<Config>> DeleteAsync(DeleteConfigurationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var config = await _configService.GetAsync(command.Id, command.Environment);
        if (config == null) return ApplicationResult<Config>.Failure(ApplicationError.NotFound);

        config.EditStatus = EditStatus.Deleted;
        config.OnlineStatus = OnlineStatus.WaitPublish;

        var isPublished = await _configService.IsPublishedAsync(config.Id, command.Environment);
        if (!isPublished) config.Status = ConfigStatus.Deleted;

        var persisted = await _configService.UpdateAsync(config, command.Environment);
        if (!persisted) return ApplicationResult<Config>.Failure(ApplicationError.OperationFailed);

        _eventBus.Fire(new DeleteConfigSuccessful(config, _currentUserAccessor.UserName));
        return ApplicationResult<Config>.Success(config);
    }

    public async Task<IReadOnlyList<Config>> GetAllAsync(string applicationId, string environment)
    {
        var configs = await _configService.GetByAppIdAsync(applicationId, environment);
        return configs
            .Where(x => x.Status != ConfigStatus.Deleted && x.EditStatus != EditStatus.Deleted)
            .ToList();
    }

    public async Task<Config> GetAsync(string applicationId, string environment, string configurationId)
    {
        var config = await _configService.GetAsync(configurationId, environment);
        if (config == null || config.AppId != applicationId ||
            config.Status == ConfigStatus.Deleted || config.EditStatus == EditStatus.Deleted)
            return null;

        return config;
    }

    private static bool IsOnlyUpdateDescription(Config newConfig, Config oldConfig)
    {
        return newConfig.Key == oldConfig.Key &&
               newConfig.Group == oldConfig.Group &&
               newConfig.Value == oldConfig.Value;
    }
}
