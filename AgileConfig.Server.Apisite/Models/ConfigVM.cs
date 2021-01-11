using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class ConfigVM
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "应用Id不能为空")]
        [MaxLength(36, ErrorMessage = "应用Id长度不能超过36位")]
        public string AppId { get; set; }

        [MaxLength(100, ErrorMessage = "配置组长度不能超过100位")]
        public string Group { get; set; }

        [Required(ErrorMessage = "配置键不能为空")]
        [MaxLength(100, ErrorMessage = "配置键长度不能超过100位")]
        public string Key { get; set; }

        [Required(ErrorMessage = "配置值不能为空")]
        [MaxLength(4000, ErrorMessage = "配置值长度不能超过4000位")]
        public string Value { get; set; }

        [MaxLength(200, ErrorMessage = "描述长度不能超过200位")]
        public string Description { get; set; }

        public OnlineStatus OnlineStatus { get; set; }
        public ConfigStatus Status { get; set; }
    }
}
