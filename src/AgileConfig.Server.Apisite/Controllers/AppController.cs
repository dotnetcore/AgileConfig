using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class AppController : Controller
{
    private readonly IAppService _appService;
    private readonly IConfigService _configService;
    private readonly ISettingService _settingService;
    private readonly ITinyEventBus _tinyEventBus;
    private readonly IUserService _userService;

    public AppController(IAppService appService,
        IUserService userService,
        IConfigService configService,
        ISettingService settingService,
        ITinyEventBus tinyEventBus)
    {
        _userService = userService;
        _configService = configService;
        _settingService = settingService;
        _tinyEventBus = tinyEventBus;
        _appService = appService;
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.App_Read })]
    public async Task<IActionResult> Search(string name, string id, string group, string sortField,
        string ascOrDesc, bool tableGrouped, int current = 1, int pageSize = 20)
    {
        if (current < 1) throw new ArgumentException(Messages.CurrentCannotBeLessThanOne);

        if (pageSize < 1) throw new ArgumentException(Messages.PageSizeCannotBeLessThanOne);

        var currentUserId = await this.GetCurrentUserId(_userService);
        var isAdmin = false;
        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            var roles = await _userService.GetUserRolesAsync(currentUserId);
            isAdmin = roles.Any(r => r.Id == SystemRoleConstants.AdminId || r.Id == SystemRoleConstants.SuperAdminId);
        }

        var appListVms = new List<AppListVM>();
        long count = 0;
        if (!tableGrouped)
        {
            var searchResult =
                await _appService.SearchAsync(id, name, group, sortField, ascOrDesc, current, pageSize, currentUserId,
                    isAdmin);
            foreach (var app in searchResult.Apps) appListVms.Add(app.ToAppListVM());

            count = searchResult.Count;
        }
        else
        {
            var searchResult =
                await _appService.SearchGroupedAsync(id, name, group, sortField, ascOrDesc, current, pageSize,
                    currentUserId, isAdmin);
            foreach (var groupedApp in searchResult.GroupedApps)
            {
                var app = groupedApp.App;
                var vm = app.ToAppListVM();
                vm.children = new List<AppListVM>();
                foreach (var child in groupedApp.Children ?? []) vm.children.Add(child.App.ToAppListVM());

                appListVms.Add(vm);
            }

            count = searchResult.Count;
        }

        await AppendInheritancedInfo(appListVms);

        return Json(new
        {
            current,
            pageSize,
            success = true,
            total = count,
            data = appListVms
        });
    }

    private async Task AppendInheritancedInfo(List<AppListVM> list)
    {
        foreach (var appListVm in list)
        {
            var inheritancedApps = await _appService.GetInheritancedAppsAsync(appListVm.Id);
            appListVm.inheritancedApps = appListVm.Inheritanced
                ? new List<string>()
                : inheritancedApps.Select(ia => ia.Id).ToList();
            appListVm.inheritancedAppNames = appListVm.Inheritanced
                ? new List<string>()
                : inheritancedApps.Select(ia => ia.Name).ToList();
            if (appListVm.children != null) await AppendInheritancedInfo(appListVm.children);
        }
    }

    private async Task<bool> IsCurrentUserAdmin(string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId)) return false;

        var roles = await _userService.GetUserRolesAsync(currentUserId);
        return roles.Any(r => r.Id == SystemRoleConstants.AdminId || r.Id == SystemRoleConstants.SuperAdminId);
    }

    private async Task<App> GetAuthorizedAppAsync(string appId, string currentUserId, bool isAdmin)
    {
        var app = await _appService.GetAsync(appId);
        if (app == null) return null;

        if (isAdmin) return app;

        var searchResult = await _appService.SearchAsync(appId, null, null, nameof(App.Id), "ascend", 1, 1,
            currentUserId, false);
        return searchResult.Apps.FirstOrDefault(x => x.Id == appId);
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Add })]
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AppVM model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var oldApp = await _appService.GetAsync(model.Id);
        if (oldApp != null)
            return Json(new
            {
                success = false,
                message = Messages.AppIdExists
            });

        var app = model.ToApp();
        app.CreateTime = DateTime.Now;
        var creatorId = await this.GetCurrentUserId(_userService);
        if (!string.IsNullOrWhiteSpace(creatorId)) app.Creator = creatorId;

        var inheritanceApps = new List<AppInheritanced>();
        if (!model.Inheritanced && model.inheritancedApps != null)
        {
            var sort = 0;
            model.inheritancedApps.ForEach(appId =>
            {
                inheritanceApps.Add(new AppInheritanced
                {
                    Id = Guid.NewGuid().ToString("N"),
                    AppId = app.Id,
                    InheritancedAppId = appId,
                    Sort = sort++
                });
            });
        }

        var result = await _appService.AddAsync(app, inheritanceApps);
        return Json(new
        {
            data = app,
            success = result,
            message = !result ? Messages.CreateAppFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Edit })]
    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] AppVM model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var app = await _appService.GetAsync(model.Id);
        if (app == null)
            return Json(new
            {
                success = false,
                message = Messages.AppNotFound
            });

        if (Appsettings.IsPreviewMode && app.Name == "test_app")
            return Json(new
            {
                success = false,
                message = Messages.DemoModeNoTestAppEdit
            });

        app = model.ToApp(app);
        app.UpdateTime = DateTime.Now;
        var inheritanceApps = new List<AppInheritanced>();
        if (!model.Inheritanced && model.inheritancedApps != null)
        {
            var sort = 0;
            model.inheritancedApps.ForEach(appId =>
            {
                inheritanceApps.Add(new AppInheritanced
                {
                    Id = Guid.NewGuid().ToString("N"),
                    AppId = app.Id,
                    InheritancedAppId = appId,
                    Sort = sort++
                });
            });
        }

        var result = await _appService.UpdateAsync(app, inheritanceApps);
        return Json(new
        {
            success = result,
            message = !result ? Messages.UpdateAppFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Read })]
    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var app = await _appService.GetAsync(id);
        var vm = app.ToAppVM();

        if (vm != null)
            vm.inheritancedApps = (await _appService.GetInheritancedAppsAsync(id)).Select(x => x.Id).ToList();
        else
            return NotFound(new
            {
                success = false,
                message = Messages.AppNotFound
            });

        return Json(new
        {
            success = true,
            data = vm
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute),
        Arguments = new object[] { Functions.App_Edit })]
    [HttpPost]
    public async Task<IActionResult> DisableOrEnable(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var app = await _appService.GetAsync(id);
        if (app == null)
            return NotFound(new
            {
                success = false,
                message = Messages.AppNotFound
            });

        app.Enabled = !app.Enabled;
        var result = await _appService.UpdateAsync(app);

        return Json(new
        {
            success = result,
            message = !result ? Messages.UpdateAppFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Delete })]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var app = await _appService.GetAsync(id);
        if (app == null)
            return NotFound(new
            {
                success = false,
                message = Messages.AppNotFound
            });

        var result = await _appService.DeleteAsync(app);

        if (result) _tinyEventBus.Fire(new DeleteAppSuccessful(app, this.GetCurrentUserName()));

        return Json(new
        {
            success = result,
            message = !result ? Messages.UpdateAppFailed : ""
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Read })]
    [HttpPost]
    public async Task<IActionResult> Export([FromBody] AppExportRequest model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var appIds = model.AppIds?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
        if (!appIds.Any()) throw new ArgumentException("appIds");

        var currentUserId = await this.GetCurrentUserId(_userService);
        var isAdmin = await IsCurrentUserAdmin(currentUserId);

        var apps = new List<App>();
        foreach (var appId in appIds)
        {
            var app = await GetAuthorizedAppAsync(appId, currentUserId, isAdmin);
            if (app == null)
            {
                Response.StatusCode = 403;
                return new ContentResult();
            }

            apps.Add(app);
        }

        var envs = (await _settingService.GetEnvironmentList())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var exportFile = new AppExportFileVM
        {
            ExportedAt = DateTime.UtcNow
        };

        foreach (var app in apps)
        {
            var inheritancedApps = await _appService.GetInheritancedAppsAsync(app.Id);
            var exportItem = new AppExportItemVM
            {
                App = new AppExportAppVM
                {
                    Id = app.Id,
                    Name = app.Name,
                    Group = app.Group,
                    Secret = app.Secret,
                    Enabled = app.Enabled,
                    Type = (int)app.Type,
                    Inheritanced = app.Type == AppType.Inheritance,
                    InheritancedApps = inheritancedApps.Select(x => x.Id).ToList()
                }
            };

            foreach (var env in envs)
            {
                var configs = await _configService.GetByAppIdAsync(app.Id, env);
                exportItem.Envs[env] = configs
                    .OrderBy(x => x.Group ?? string.Empty, StringComparer.Ordinal)
                    .ThenBy(x => x.Key ?? string.Empty, StringComparer.Ordinal)
                    .Select(x => new AppExportConfigVM
                    {
                        Group = x.Group,
                        Key = x.Key,
                        Value = x.Value,
                        Description = x.Description
                    })
                    .ToList();
            }

            exportFile.Apps.Add(exportItem);
        }

        var json = JsonConvert.SerializeObject(exportFile, Formatting.Indented);
        var fileName = $"agileconfig-export-{DateTime.UtcNow:yyyyMMddHHmmss}.json";
        return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Add })]
    [HttpPost]
    public async Task<IActionResult> PreviewImport(IFormFile file)
    {
        var importFile = await ReadImportFileAsync(file);
        var preview = await BuildImportPreviewAsync(importFile);
        return Json(new
        {
            success = !preview.Errors.Any(),
            data = preview,
            message = preview.Errors.FirstOrDefault()
        });
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Add })]
    [HttpPost]
    public async Task<IActionResult> Import([FromBody] AppImportRequest model)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(model.File);

        var preview = await BuildImportPreviewAsync(model.File);
        if (preview.Errors.Any())
            return Json(new
            {
                success = false,
                data = preview,
                message = string.Join(Environment.NewLine, preview.Errors)
            });

        var currentUserId = await this.GetCurrentUserId(_userService);
        var now = DateTime.Now;

        foreach (var previewItem in preview.Apps.OrderBy(x => x.Order))
        {
            var importItem = model.File.Apps.First(x => string.Equals(x.App?.Id, previewItem.AppId, StringComparison.OrdinalIgnoreCase));
            var app = new App
            {
                Id = importItem.App.Id,
                Name = importItem.App.Name,
                Group = importItem.App.Group,
                Secret = importItem.App.Secret,
                Enabled = importItem.App.Enabled,
                Type = importItem.App.Inheritanced ? AppType.Inheritance : AppType.PRIVATE,
                CreateTime = now,
                Creator = currentUserId
            };

            var inheritanceApps = BuildInheritanceLinks(importItem.App.InheritancedApps, app.Id);
            await _appService.AddAsync(app, inheritanceApps);

            foreach (var envConfigs in importItem.Envs)
            {
                foreach (var configVm in envConfigs.Value ?? new List<AppExportConfigVM>())
                {
                    var config = new Config
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        AppId = app.Id,
                        Env = envConfigs.Key,
                        Group = configVm.Group,
                        Key = configVm.Key,
                        Value = configVm.Value,
                        Description = configVm.Description,
                        CreateTime = now,
                        Status = ConfigStatus.Enabled,
                        OnlineStatus = OnlineStatus.WaitPublish,
                        EditStatus = EditStatus.Add
                    };
                    await _configService.AddAsync(config, envConfigs.Key);
                }
            }
        }

        return Json(new
        {
            success = true,
            data = preview
        });
    }

    private static List<AppInheritanced> BuildInheritanceLinks(List<string> parentIds, string appId)
    {
        var inheritanceApps = new List<AppInheritanced>();
        if (parentIds == null) return inheritanceApps;

        var sort = 0;
        foreach (var parentId in parentIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
            inheritanceApps.Add(new AppInheritanced
            {
                Id = Guid.NewGuid().ToString("N"),
                AppId = appId,
                InheritancedAppId = parentId,
                Sort = sort++
            });

        return inheritanceApps;
    }

    private async Task<AppExportFileVM> ReadImportFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("file");

        using var stream = file.OpenReadStream();
        using var reader = new System.IO.StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync();
        var importFile = JsonConvert.DeserializeObject<AppExportFileVM>(content);
        if (importFile == null) throw new ArgumentException("file");

        return importFile;
    }

    private async Task<AppImportPreviewVM> BuildImportPreviewAsync(AppExportFileVM importFile)
    {
        var preview = new AppImportPreviewVM();
        if (importFile?.Apps == null || !importFile.Apps.Any())
        {
            preview.Errors.Add("Import file does not contain any apps.");
            return preview;
        }

        var appItems = importFile.Apps
            .Where(x => x?.App != null)
            .ToList();
        if (!appItems.Any())
        {
            preview.Errors.Add("Import file does not contain any valid app entries.");
            return preview;
        }

        var duplicateIds = appItems
            .GroupBy(x => x.App.Id ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Count() > 1)
            .Select(x => x.Key)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        preview.Errors.AddRange(duplicateIds.Select(x => $"Duplicate AppId in import file: {x}."));

        var duplicateNames = appItems
            .GroupBy(x => x.App.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && x.Count() > 1)
            .Select(x => x.Key)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        preview.Errors.AddRange(duplicateNames.Select(x => $"Duplicate app name in import file: {x}."));

        foreach (var item in appItems)
        {
            if (string.IsNullOrWhiteSpace(item.App.Id)) preview.Errors.Add("Imported app is missing AppId.");
            if (string.IsNullOrWhiteSpace(item.App.Name)) preview.Errors.Add("Imported app is missing Name.");
        }

        var importedAppIds = new HashSet<string>(appItems.Select(x => x.App.Id).Where(x => !string.IsNullOrWhiteSpace(x)), StringComparer.OrdinalIgnoreCase);
        var existingApps = await _appService.GetAllAppsAsync();

        foreach (var item in appItems.Where(x => x.App != null && !string.IsNullOrWhiteSpace(x.App.Id)))
        {
            if (existingApps.Any(x => string.Equals(x.Id, item.App.Id, StringComparison.OrdinalIgnoreCase)))
                preview.Errors.Add($"AppId already exists: {item.App.Id}.");
            if (!string.IsNullOrWhiteSpace(item.App.Name) && existingApps.Any(x => string.Equals(x.Name, item.App.Name, StringComparison.OrdinalIgnoreCase)))
                preview.Errors.Add($"App name already exists: {item.App.Name}.");
        }

        foreach (var item in appItems)
        {
            foreach (var parentId in item.App.InheritancedApps?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase) ?? new List<string>())
            {
                if (importedAppIds.Contains(parentId)) continue;
                if (existingApps.Any(x => string.Equals(x.Id, parentId, StringComparison.OrdinalIgnoreCase))) continue;
                preview.Errors.Add($"App '{item.App.Id}' references missing parent '{parentId}'. Parent must already exist or be included in the import file.");
            }
        }

        var orderLookup = TryTopologicalSort(appItems, importedAppIds, preview.Errors);
        if (preview.Errors.Any()) return preview;

        preview.Apps = appItems
            .OrderBy(x => orderLookup[x.App.Id])
            .Select(x => new AppImportPreviewItemVM
            {
                AppId = x.App.Id,
                Name = x.App.Name,
                Group = x.App.Group,
                Enabled = x.App.Enabled,
                Inheritanced = x.App.Inheritanced,
                InheritancedApps = x.App.InheritancedApps?.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>(),
                EnvCount = x.Envs?.Count ?? 0,
                ConfigCount = x.Envs?.Sum(env => env.Value?.Count ?? 0) ?? 0,
                Order = orderLookup[x.App.Id]
            })
            .ToList();

        return preview;
    }

    private static Dictionary<string, int> TryTopologicalSort(List<AppExportItemVM> appItems, HashSet<string> importedAppIds, List<string> errors)
    {
        var dependencyMap = appItems.ToDictionary(
            x => x.App.Id,
            x => (x.App.InheritancedApps ?? new List<string>())
                .Where(parentId => !string.IsNullOrWhiteSpace(parentId) && importedAppIds.Contains(parentId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            StringComparer.OrdinalIgnoreCase);

        var inDegree = dependencyMap.ToDictionary(x => x.Key, _ => 0, StringComparer.OrdinalIgnoreCase);
        var childMap = dependencyMap.Keys.ToDictionary(x => x, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in dependencyMap)
        {
            inDegree[entry.Key] = entry.Value.Count;
            foreach (var parentId in entry.Value) childMap[parentId].Add(entry.Key);
        }

        var queue = new Queue<string>(inDegree.Where(x => x.Value == 0).Select(x => x.Key).OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
        var ordered = new List<string>();
        while (queue.Any())
        {
            var next = queue.Dequeue();
            ordered.Add(next);

            foreach (var child in childMap[next].OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                inDegree[child]--;
                if (inDegree[child] == 0) queue.Enqueue(child);
            }
        }

        if (ordered.Count != dependencyMap.Count)
        {
            var cyclicApps = inDegree.Where(x => x.Value > 0).Select(x => x.Key).OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
            errors.Add($"Cyclic inheritance detected among imported apps: {string.Join(", ", cyclicApps)}.");
        }

        return ordered.Select((appId, index) => new { appId, index }).ToDictionary(x => x.appId, x => x.index + 1, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Get all applications that can be inherited.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<IActionResult> InheritancedApps(string currentAppId)
    {
        var apps = await _appService.GetAllInheritancedAppsAsync();
        apps = apps.Where(a => a.Enabled).ToList();
        var self = apps.FirstOrDefault(a => a.Id == currentAppId);
        if (self != null)
            // Exclude the current application itself.
            apps.Remove(self);

        var vms = apps.Select(x =>
        {
            return new
            {
                x.Id, x.Name
            };
        });

        return Json(new
        {
            success = true,
            data = vms
        });
    }

    /// <summary>
    ///     Save application authorization information.
    /// </summary>
    /// <param name="model">View model containing authorization assignments.</param>
    /// <returns>Operation result.</returns>
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Auth })]
    [HttpPost]
    public async Task<IActionResult> SaveAppAuth([FromBody] AppAuthVM model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await _appService.SaveUserAppAuth(model.AppId, model.AuthorizedUsers);

        return Json(new
        {
            success = result
        });
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<IActionResult> GetUserAppAuth(string appId)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var result = new AppAuthVM
        {
            AppId = appId
        };
        result.AuthorizedUsers =
            (await _appService.GetUserAppAuth(appId)).Select(x => x.Id).ToList();

        return Json(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet]
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { Functions.App_Read })]
    public async Task<IActionResult> GetAppGroups()
    {
        var groups = await _appService.GetAppGroups();
        return Json(new
        {
            success = true,
            data = groups.OrderBy(x => x)
        });
    }
}
