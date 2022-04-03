using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Models
{

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

    public class AppListVM : AppVM
    {

        public DateTime? UpdateTime { get; set; }
        
        public List<AppListVM> children { get; set; }
    }
}