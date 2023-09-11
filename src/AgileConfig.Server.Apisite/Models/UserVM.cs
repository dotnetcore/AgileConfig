using AgileConfig.Server.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class UserVM
    {
        [Required(ErrorMessage = "用户Id不能为空")]
        [MaxLength(36, ErrorMessage = "用户Id长度不能超过36位")]
        public string Id { get; set; }

        [Required(ErrorMessage = "用户名不能为空")]
        [MaxLength(50, ErrorMessage = "用户名长度不能超过50位")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [MaxLength(50, ErrorMessage = "密码长度不能超过50位")]
        public string Password { get; set; }

        [MaxLength(50, ErrorMessage = "团队长度不能超过50位")]
        public string Team { get; set; }

        public List<Role> UserRoles { get; set; }

        public List<string> UserRoleNames { get; set; }

        public UserStatus Status { get; set; }
    }
}
