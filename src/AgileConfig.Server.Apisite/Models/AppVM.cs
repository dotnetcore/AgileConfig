using AgileConfig.Server.Apisite.Controllers.api.Models;
using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    public class AppVM : IAppModel
    {
        [Required(ErrorMessage = "应用Id不能为空")]
        [MaxLength(36, ErrorMessage = "应用Id长度不能超过36位")]
        public string Id { get; set; }

        [Required(ErrorMessage = "应用名称不能为空")]
        [MaxLength(50, ErrorMessage = "应用名称长度不能超过50位")]
        public string Name { get; set; }

        [MaxLength(50, ErrorMessage = "应用组名称长度不能超过50位")]
        public string Group { get; set; }

        [MaxLength(36, ErrorMessage = "密钥长度不能超过36位")]
        public string Secret { get; set; }

        public bool Enabled { get; set; }
        public bool Inheritanced { get; set; }

        public List<string> inheritancedApps { get; set; }

        public List<string> inheritancedAppNames { get; set; }

        public string AppAdmin { get; set; }

        public string AppAdminName { get; set; }
        
        public DateTime CreateTime { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class AppListVM : AppVM
    {

        public DateTime? UpdateTime { get; set; }
        
        public List<AppListVM> children { get; set; }
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
}