using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using AgileConfig.Server.Apisite.Utilites;
using AgileConfig.Server.Common.EventBus;
using AgileConfig.Server.Event;
using AgileConfig.Server.Apisite.Models.Mapping;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    [ModelVaildate]
    public class AppController : Controller
    {
        private readonly IAppService _appService;
        private readonly IPremissionService _premissionService;
        private readonly IUserService _userService;
        private readonly ITinyEventBus _tinyEventBus;

        public AppController(IAppService appService,
            IPremissionService premissionService,
            IUserService userService,
            ITinyEventBus tinyEventBus)
        {
            _userService = userService;
            _tinyEventBus = tinyEventBus;
            _appService = appService;
            _premissionService = premissionService;
        }

        public async Task<IActionResult> Search(string name, string id, string group, string sortField, string ascOrDesc, bool tableGrouped, int current = 1, int pageSize = 20)
        {
            if (current < 1)
            {
                throw new ArgumentException("current cant less then 1 .");
            }
            if (pageSize < 1)
            {
                throw new ArgumentException("pageSize cant less then 1 .");
            }

            var query = await _appService.GetAllAppsAsync();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(id))
            {
                query = query.Where(x => x.Id.Contains(id)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(group))
            {
                query = query.Where(x => x.Group == group).ToList();
            }

            var appvms = new List<AppListVM>();
            foreach (var app in query)
            {
                appvms.Add(await AppToListVM(app, false));
            }
            if (tableGrouped)
            {
                var appGroups = appvms.GroupBy(x => x.Group);
                var appGroupList = new List<AppListVM>();
                foreach (var appGroup in appGroups)
                {
                    var first = appGroup.First();
                    var children = new List<AppListVM>();
                    if (appGroup.Count() > 1)
                    {
                        foreach (var item in appGroup)
                        {
                            if (first.Id != item.Id)
                            {
                                children.Add(item);
                            }
                        }
                    }

                    if (children.Count > 0)
                    {
                        first.children = children;
                    }
                    appGroupList.Add(first);
                }

                appvms = appGroupList;
            }

            if (tableGrouped)
            {
                if (sortField == "group" && ascOrDesc.StartsWith("desc"))
                {
                    appvms = appvms.OrderByDescending(x => x.Group).ToList();
                }
                else
                {
                    appvms = appvms.OrderBy(x => x.Group).ToList();
                }
            }
            else
            {
                if (sortField == "createTime")
                {
                    if (ascOrDesc.StartsWith("asc"))
                    {
                        appvms = appvms.OrderBy(x => x.CreateTime).ToList();
                    }
                    else
                    {
                        appvms = appvms.OrderByDescending(x => x.CreateTime).ToList();
                    }
                }
                if (sortField == "id")
                {
                    if (ascOrDesc.StartsWith("asc"))
                    {
                        appvms = appvms.OrderBy(x => x.Id).ToList();
                    }
                    else
                    {
                        appvms = appvms.OrderByDescending(x => x.Id).ToList();
                    }
                }
                if (sortField == "name")
                {
                    if (ascOrDesc.StartsWith("asc"))
                    {
                        appvms = appvms.OrderBy(x => x.Name).ToList();
                    }
                    else
                    {
                        appvms = appvms.OrderByDescending(x => x.Name).ToList();
                    }
                }
                if (sortField == "group")
                {
                    if (ascOrDesc.StartsWith("asc"))
                    {
                        appvms = appvms.OrderBy(x => x.Group).ToList();
                    }
                    else
                    {
                        appvms = appvms.OrderByDescending(x => x.Group).ToList();
                    }
                }
            }

            var count = appvms.Count;
            var pageList = appvms.ToList().Skip((current - 1) * pageSize).Take(pageSize).ToList();
            await AppendInheritancedInfo(pageList);
            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = count,
                data = pageList
            });
        }

        private async Task<AppListVM> AppToListVM(App item, bool appendInheritancedInfo)
        {
            var vm = item.ToAppListVM();

            if (appendInheritancedInfo)
            {
                var inheritancedApps = await _appService.GetInheritancedAppsAsync(item.Id);
                vm.inheritancedApps = item.Type == AppType.Inheritance
                    ? new List<string>()
                    : (inheritancedApps).Select(ia => ia.Id).ToList();
                vm.inheritancedAppNames = item.Type == AppType.Inheritance
                    ? new List<string>()
                    : (inheritancedApps).Select(ia => ia.Name).ToList();
                vm.AppAdminName = (await _userService.GetUserAsync(item.AppAdmin))?.UserName;
            }

            return vm;
        }

        private async Task AppendInheritancedInfo(List<AppListVM> list)
        {
            foreach (var appListVm in list)
            {
                var inheritancedApps = await _appService.GetInheritancedAppsAsync(appListVm.Id);
                appListVm.inheritancedApps = appListVm.Inheritanced
                    ? new List<string>()
                    : (inheritancedApps).Select(ia => ia.Id).ToList();
                appListVm.inheritancedAppNames = appListVm.Inheritanced
                    ? new List<string>()
                    : (inheritancedApps).Select(ia => ia.Name).ToList();
                appListVm.AppAdminName = (await _userService.GetUserAsync(appListVm.AppAdmin))?.UserName;
                if (appListVm.children != null)
                {
                    await AppendInheritancedInfo(appListVm.children);
                }
            }
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "App.Add", Functions.App_Add })]
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AppVM model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var oldApp = await _appService.GetAsync(model.Id);
            if (oldApp != null)
            {

                return Json(new
                {
                    success = false,
                    message = "应用Id已存在，请重新输入。"
                });
            }

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
            if (result)
            {
                _tinyEventBus.Fire(new AddAppSuccessful(app, this.GetCurrentUserName()));
            }

            return Json(new
            {
                data = app,
                success = result,
                message = !result ? "新建应用失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "App.Edit", Functions.App_Edit })]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] AppVM model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var app = await _appService.GetAsync(model.Id);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的应用程序。"
                });
            }

            if (Appsettings.IsPreviewMode && app.Name == "test_app")
            {
                return Json(new
                {
                    success = false,
                    message = "演示模式请勿修改Test_App"
                });
            }

            model.ToApp(app);
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
            if (result)
            {
                _tinyEventBus.Fire(new EditAppSuccessful(app, this.GetCurrentUserName()));
            }
            return Json(new
            {
                success = result,
                message = !result ? "修改应用失败，请查看错误日志" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var apps = await _appService.GetAllAppsAsync();
            var vms = new List<AppListVM>();
            foreach (var app in apps)
            {
                var vm = app.ToAppListVM();
                vm.inheritancedAppNames = app.Type == AppType.Inheritance ? new List<string>() :
                                                                            (await _appService.GetInheritancedAppsAsync(app.Id)).Select(ia => ia.Id).ToList();
                vms.Add(vm);
            }

            return Json(new
            {
                success = true,
                data = vms
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(id);

            var app = await _appService.GetAsync(id);
            var vm = app.ToAppVM();

            if (vm != null)
            {
                vm.inheritancedApps = (await _appService.GetInheritancedAppsAsync(id)).Select(x => x.Id).ToList();
            }

            return Json(new
            {
                success = app != null,
                data = vm,
                message = app == null ? "未找到对应的应用程序。" : ""
            });
        }

        /// <summary>
        /// 在启动跟禁用之间进行切换
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "App.DisableOrEanble", Functions.App_Edit })]
        [HttpPost]
        public async Task<IActionResult> DisableOrEanble(string id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(id);

            var app = await _appService.GetAsync(id);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的应用程序。"
                });
            }

            app.Enabled = !app.Enabled;

            var result = await _appService.UpdateAsync(app);

            if (result)
            {
                _tinyEventBus.Fire(new DisableOrEnableAppSuccessful(app, this.GetCurrentUserName()));
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改应用失败，请查看错误日志" : ""
            });
        }

        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "App.Delete", Functions.App_Delete })]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(id);

            var app = await _appService.GetAsync(id);
            if (app == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的应用程序。"
                });
            }

            var result = await _appService.DeleteAsync(app);

            if (result)
            {
                _tinyEventBus.Fire(new DeleteAppSuccessful(app, this.GetCurrentUserName()));
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改应用失败，请查看错误日志" : ""
            });
        }

        /// <summary>
        /// 获取所有可以继承的app
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> InheritancedApps(string currentAppId)
        {
            var apps = await _appService.GetAllInheritancedAppsAsync();
            apps = apps.Where(a => a.Enabled).ToList();
            var self = apps.FirstOrDefault(a => a.Id == currentAppId);
            if (self != null)
            {
                //过滤本身
                apps.Remove(self);
            }
            var vms = apps.Select(x =>
            {
                return new
                {
                    Id = x.Id,
                    Name = x.Name,
                };
            });

            return Json(new
            {
                success = true,
                data = vms
            });
        }

        /// <summary>
        /// 保存app的授权信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [TypeFilter(typeof(PremissionCheckAttribute), Arguments = new object[] { "App.Auth", Functions.App_Auth })]
        [HttpPost]
        public async Task<IActionResult> SaveAppAuth([FromBody] AppAuthVM model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var result = await _appService.SaveUserAppAuth(model.AppId, model.EditConfigPermissionUsers, _premissionService.EditConfigPermissionKey);
            var result1 = await _appService.SaveUserAppAuth(model.AppId, model.PublishConfigPermissionUsers, _premissionService.PublishConfigPermissionKey);

            return Json(new
            {
                success = result && result1
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserAppAuth(string appId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(appId);

            var result = new AppAuthVM
            {
                AppId = appId
            };
            result.EditConfigPermissionUsers = (await _appService.GetUserAppAuth(appId, _premissionService.EditConfigPermissionKey)).Select(x => x.Id).ToList();
            result.PublishConfigPermissionUsers = (await _appService.GetUserAppAuth(appId, _premissionService.PublishConfigPermissionKey)).Select(x => x.Id).ToList();

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
}
