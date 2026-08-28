using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;

namespace AgileConfig.Server.Application;

public sealed class ApplicationManagementService : IApplicationManagementService
{
    private readonly IAppService _appService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITinyEventBus _eventBus;
    private readonly IPreviewModeAccessor _previewModeAccessor;
    private readonly TimeProvider _timeProvider;

    public ApplicationManagementService(
        IAppService appService,
        ITinyEventBus eventBus,
        ICurrentUserAccessor currentUserAccessor,
        TimeProvider timeProvider,
        IPreviewModeAccessor previewModeAccessor)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _previewModeAccessor = previewModeAccessor ?? throw new ArgumentNullException(nameof(previewModeAccessor));
    }

    public async Task<IReadOnlyList<ApplicationDetails>> GetAllAsync()
    {
        var applications = await _appService.GetAllAppsAsync();
        var result = new List<ApplicationDetails>(applications.Count);
        foreach (var application in applications)
        {
            result.Add(await BuildDetailsAsync(application));
        }

        return result;
    }

    public async Task<ApplicationDetails> GetAsync(string applicationId)
    {
        var application = await _appService.GetAsync(applicationId);
        return application == null ? null : await BuildDetailsAsync(application);
    }

    public async Task<ApplicationResult<App>> CreateAsync(CreateApplicationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await _appService.GetAsync(command.Id) != null)
            return ApplicationResult<App>.Failure(ApplicationError.Conflict);

        if (command.ValidateInheritanceReferences &&
            !await HasValidInheritanceReferencesAsync(command.Id, command.IsInheritanceSource, command.InheritsFrom))
            return ApplicationResult<App>.Failure(ApplicationError.ValidationFailed);

        var app = new App
        {
            Id = command.Id,
            Name = command.Name,
            Group = command.Group,
            Secret = command.Secret,
            Enabled = command.Enabled,
            Type = command.IsInheritanceSource ? AppType.Inheritance : AppType.PRIVATE,
            CreateTime = GetLocalNow(),
            Creator = await GetCurrentUserIdAsync()
        };

        var inheritanceApps = BuildInheritanceApps(app.Id, command.IsInheritanceSource, command.InheritsFrom);
        var succeeded = await _appService.AddAsync(app, inheritanceApps);
        if (!succeeded) return ApplicationResult<App>.Failure(ApplicationError.OperationFailed, app);

        return ApplicationResult<App>.Success(app);
    }

    public async Task<ApplicationResult<App>> UpdateAsync(UpdateApplicationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var app = await _appService.GetAsync(command.Id);
        if (app == null) return ApplicationResult<App>.Failure(ApplicationError.NotFound);

        if (_previewModeAccessor.IsPreviewMode && app.Name == "test_app")
            return ApplicationResult<App>.Failure(ApplicationError.ValidationFailed);

        if (command.ValidateInheritanceReferences &&
            !await HasValidInheritanceReferencesAsync(command.Id, command.IsInheritanceSource, command.InheritsFrom))
            return ApplicationResult<App>.Failure(ApplicationError.ValidationFailed);

        app.Name = command.Name;
        app.Group = command.Group;
        app.Secret = command.Secret;
        app.Enabled = command.Enabled;
        app.Type = command.IsInheritanceSource ? AppType.Inheritance : AppType.PRIVATE;
        app.UpdateTime = GetLocalNow();

        var inheritanceApps = BuildInheritanceApps(app.Id, command.IsInheritanceSource, command.InheritsFrom);
        var succeeded = await _appService.UpdateAsync(app, inheritanceApps);
        if (!succeeded) return ApplicationResult<App>.Failure(ApplicationError.OperationFailed);

        return ApplicationResult<App>.Success(app);
    }

    public async Task<ApplicationResult<App>> DeleteAsync(DeleteApplicationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var app = await _appService.GetAsync(command.Id);
        if (app == null) return ApplicationResult<App>.Failure(ApplicationError.NotFound);

        var succeeded = await _appService.DeleteAsync(app);
        if (!succeeded) return ApplicationResult<App>.Failure(ApplicationError.OperationFailed);

        _eventBus.Fire(new DeleteAppSuccessful(app, _currentUserAccessor.UserName));
        return ApplicationResult<App>.Success(app);
    }

    private DateTime GetLocalNow()
    {
        return _timeProvider.GetLocalNow().DateTime;
    }

    private async Task<ApplicationDetails> BuildDetailsAsync(App application)
    {
        var inheritedApplications = await _appService.GetInheritancedAppsAsync(application.Id);
        return new ApplicationDetails(
            application,
            inheritedApplications.Select(x => x.Id).ToList());
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        var userId = await _currentUserAccessor.GetUserIdAsync();
        return string.IsNullOrWhiteSpace(userId) ? null : userId;
    }

    private async Task<bool> HasValidInheritanceReferencesAsync(
        string applicationId,
        bool isInheritanceSource,
        IReadOnlyList<string> inheritedAppIds)
    {
        if (isInheritanceSource || inheritedAppIds == null || inheritedAppIds.Count == 0) return true;

        var distinctIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var inheritedAppId in inheritedAppIds)
        {
            if (string.IsNullOrWhiteSpace(inheritedAppId) ||
                inheritedAppId.Length > 36 ||
                !distinctIds.Add(inheritedAppId) ||
                string.Equals(applicationId, inheritedAppId, StringComparison.Ordinal))
                return false;

            var inheritedApp = await _appService.GetAsync(inheritedAppId);
            if (inheritedApp?.Type != AppType.Inheritance) return false;
        }

        return true;
    }

    private static List<AppInheritanced> BuildInheritanceApps(
        string appId,
        bool isInheritanceSource,
        IReadOnlyList<string> inheritedAppIds)
    {
        if (isInheritanceSource || inheritedAppIds == null) return [];

        return inheritedAppIds
            .Select((inheritedAppId, index) => new AppInheritanced
            {
                Id = Guid.NewGuid().ToString("N"),
                AppId = appId,
                InheritancedAppId = inheritedAppId,
                Sort = index
            })
            .ToList();
    }
}
