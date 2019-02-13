using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Models
{
    public class EditAppVM
    {
        [Required]
        [MaxLength(8,ErrorMessage ="应用Id长度不能超过8位")]
        public string Id { get; set; }
        [Required]
        [MaxLength(20, ErrorMessage = "应用名称长度不能超过20位")]
        public string Name { get; set; }
        [MaxLength(36, ErrorMessage = "密钥长度不能超过36位")]
        public string Secret { get; set; }
        public bool Enabled { get; set; }
    }
}
