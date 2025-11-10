using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Models.Mapping;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Common.Resources;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Event;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers;

[Authorize]
[ModelVaildate]
public class AppController : Controller
{
    private readonly IAppService _appService;
    private readonly IPermissionService _permissionService;
    private readonly ITinyEventBus _tinyEventBus;
    private readonly IUserService _userService;

    public AppController(IAppService appService,
        IPermissionService permissionService,
        IUserService userService,
        ITinyEventBus tinyEventBus)
    {
        _userService = userService;
        _tinyEventBus = tinyEventBus;
        _appService = appService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Search(string name, string id, string group, string sortField,
        string ascOrDesc, bool tableGrouped, int current = 1, int pageSize = 20)
    {
        if (current < 1) throw new ArgumentException(Messages.CurrentCannotBeLessThanOne);

        if (pageSize < 1) throw new ArgumentException(Messages.PageSizeCannotBeLessThanOne);

        var appListVms = new List<AppListVM>();
        long count = 0;
        if (!tableGrouped)
        {
            var searchResult =
                await _appService.SearchAsync(id, name, group, sortField, ascOrDesc, current, pageSize);
            foreach (var app in searchResult.Apps) appListVms.Add(app.ToAppListVM());

            count = searchResult.Count;
        }
        else
        {
            var searchResult =
                await _appService.SearchGroupedAsync(id, name, group, sortField, ascOrDesc, current, pageSize);
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
            appListVm.AppAdminName = (await _userService.GetUserAsync(appListVm.AppAdmin))?.UserName;
            if (appListVm.children != null) await AppendInheritancedInfo(appListVm.children);
        }
    }

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "App.Add", Functions.App_Add })]
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

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "App.Edit", Functions.App_Edit })]
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
        Arguments = new object[] { "App.DisableOrEnable", Functions.App_Edit })]
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

    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "App.Delete", Functions.App_Delete })]
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

    /// <summary>
    ///     Get all applications that can be inherited.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
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
    [TypeFilter(typeof(PermissionCheckAttribute), Arguments = new object[] { "App.Auth", Functions.App_Auth })]
    [HttpPost]
    public async Task<IActionResult> SaveAppAuth([FromBody] AppAuthVM model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var result = await _appService.SaveUserAppAuth(model.AppId, model.EditConfigPermissionUsers,
            _permissionService.EditConfigPermissionKey);
        var result1 = await _appService.SaveUserAppAuth(model.AppId, model.PublishConfigPermissionUsers,
            _permissionService.PublishConfigPermissionKey);

        return Json(new
        {
            success = result && result1
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAppAuth(string appId)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        var result = new AppAuthVM
        {
            AppId = appId
        };
        result.EditConfigPermissionUsers =
            (await _appService.GetUserAppAuth(appId, _permissionService.EditConfigPermissionKey)).Select(x => x.Id)
            .ToList();
        result.PublishConfigPermissionUsers =
            (await _appService.GetUserAppAuth(appId, _permissionService.PublishConfigPermissionKey))
            .Select(x => x.Id).ToList();

        return Json(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet]
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