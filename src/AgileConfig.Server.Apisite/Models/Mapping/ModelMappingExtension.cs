using AgileConfig.Server.Data.Entity;
using System;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using Google.Protobuf.WellKnownTypes;

namespace AgileConfig.Server.Apisite.Models.Mapping
{
    /// <summary>
    /// Do not ask me why not use AutoMapper, I don't know. Just like manual mapping, it's simple and clear.
    /// </summary>
    public static class AppExtension
    {
        public static AppVM ToAppVM(this App app)
        {
            if (app == null)
            {
                return null;
            }

            var appVM = new AppVM
            {
                Id = app.Id,
                Name = app.Name,
                Group = app.Group,
                Secret = app.Secret,
                Enabled = app.Enabled,
                Inheritanced = app.Type == AppType.Inheritance,
                AppAdmin = app.AppAdmin,
                CreateTime = app.CreateTime,
            };

            return appVM;
        }

        public static AppListVM ToAppListVM(this App app)
        {
            if (app == null)
            {
                return null;
            }

            var vm = new AppListVM
            {
                Id = app.Id,
                Name = app.Name,
                Group = app.Group,
                Secret = app.Secret,
                Inheritanced = app.Type == AppType.Inheritance,
                Enabled = app.Enabled,
                UpdateTime = app.UpdateTime,
                CreateTime = app.CreateTime,
                AppAdmin = app.AppAdmin,
            };

            return vm;
        }

        public static ApiAppVM ToApiAppVM(this App vm)
        {
            if (vm == null)
            {
                return null;
            }

            return new ApiAppVM
            {
                Id = vm.Id,
                Name = vm.Name,
                Secret = vm.Secret,
                Inheritanced = vm.Type == AppType.Inheritance,
                Enabled = vm.Enabled,
                AppAdmin = vm.AppAdmin,
                Group = vm.Group,
                CreateTime = vm.CreateTime
            };
        }
    }

    public static class AppVMExtension
    {
        public static App ToApp(this AppVM vm)
        {
            if (vm == null)
            {
                return null;
            }

            var app = new App();
            app.Id = vm.Id;
            app.Name = vm.Name;
            app.Secret = vm.Secret;
            app.Enabled = vm.Enabled;
            app.Type = vm.Inheritanced ? AppType.Inheritance : AppType.PRIVATE;
            app.AppAdmin = vm.AppAdmin;
            app.Group = vm.Group;
            app.CreateTime = vm.CreateTime;

            return app;
        }

        public static App ToApp(this AppVM vm, App app)
        {
            if (vm == null)
            {
                return null;
            }

            app.Id = vm.Id;
            app.Name = vm.Name;
            app.Secret = vm.Secret;
            app.Enabled = vm.Enabled;
            app.Type = vm.Inheritanced ? AppType.Inheritance : AppType.PRIVATE;
            app.AppAdmin = vm.AppAdmin;
            app.Group = vm.Group;
            if (vm.CreateTime > DateTime.MinValue)
            {
                app.CreateTime = vm.CreateTime;
            }

            return app;
        }

        public static ApiAppVM ToApiAppVM(this AppVM vm)
        {
            if (vm == null)
            {
                return null;
            }

            return new ApiAppVM
            {
                Id = vm.Id,
                Name = vm.Name,
                Secret = vm.Secret,
                Inheritanced = vm.Inheritanced,
                Enabled = vm.Enabled,
                InheritancedApps = vm.inheritancedApps,
                AppAdmin = vm.AppAdmin,
                Group = vm.Group,
                CreateTime = vm.CreateTime
            };
        }
    }

    public static class PublishTimelineExtension
    {
        public static ApiPublishTimelineVM ToApiPublishTimelimeVM(this PublishTimeline timeline)
        {
            if (timeline == null)
            {
                return null;
            }

            return new ApiPublishTimelineVM
            {
                Id = timeline.Id,
                Version = timeline.Version,
                AppId = timeline.AppId,
                Log = timeline.Log,
                PublishTime = timeline.PublishTime,
                PublishUserId = timeline.PublishUserId,
                Env = timeline.Env
            };
        }
    }

    public static class ApiAppVMExtension
    {
        public static AppVM ToAppVM(this ApiAppVM vm)
        {
            if (vm == null)
            {
                return null;
            }

            return new AppVM
            {
                Id = vm.Id,
                Name = vm.Name,
                Secret = vm.Secret,
                AppAdmin = vm.AppAdmin,
                Inheritanced = vm.Inheritanced,
                Group = vm.Group
            };
        }
    }
}
