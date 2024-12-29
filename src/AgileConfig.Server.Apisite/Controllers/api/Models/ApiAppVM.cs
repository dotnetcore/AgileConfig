using AgileConfig.Server.Apisite.Models;
using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;

namespace AgileConfig.Server.Apisite.Controllers.api.Models
{
    /// <summary>
    /// restful api 返回的 app  模型
    /// </summary>
    public class ApiAppVM : IAppModel
    {
        /// <summary>
        /// 是否可继承
        /// </summary>
        public bool Inheritanced { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? Enabled { get; set; }
        /// <summary>
        /// 关联的app
        /// </summary>
        public List<string> InheritancedApps { get; set; }
        /// <summary>
        /// 管理员
        /// </summary>
        public string AppAdmin { get; set; }
        
        public string Group { get; set; }
        
        public DateTime CreateTime { get; set; }
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
                Group = vm.Group,
                Enabled = vm.Enabled.GetValueOrDefault()
            };
        }
    }
}
