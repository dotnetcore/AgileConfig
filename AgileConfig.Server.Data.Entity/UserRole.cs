using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_role")]
    public class UserRole
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "user_id", StringLength = 50)]
        public string UserId { get; set; }

        [Column(Name = "role")]
        public Role Role { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

    }

    public enum Role
    {
        SuperAdmin = 0,//系统管理员
        Admin, //app管理员
        NormalUser, //普通用户
    }

    public enum AppRole
    {
        APP_Editor = 10, //app维护
        Config_Publisher = 11, // 配置项发布下线
        Config_Editor = 12 //配置项编辑
    }

}
