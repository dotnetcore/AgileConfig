using AgileConfig.Server.Data.Entity;
using System;

namespace AgileConfig.Server.Apisite.Models.Mapping
{
    /// <summary>
    /// Do not ask me why not use AutoMapper, I don't know. Just like mannual mapping, it's simple and clear.
    /// </summary>
    public static class ModelMappingExtension
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
    }
}
