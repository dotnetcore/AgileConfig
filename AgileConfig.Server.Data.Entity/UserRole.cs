using FreeSql.DataAnnotations;
using System;
using System.ComponentModel;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_role")]
    [OraclePrimaryKeyName("agc_user_role_pk")]
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
        [Description("超级管理员")]
        SuperAdmin = 0,
        [Description("管理员")]
        Admin,
        [Description("操作员")]
        NormalUser, 
    }

    public enum AppRole
    {
        APP_Editor = 10, //app维护
        Config_Publisher = 11, // 配置项发布下线
        Config_Editor = 12 //配置项编辑
    }

}
