using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_role")]
    public class UserRole
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "app_id", StringLength = 36)]
        public string AppId { get; set; }

        [Column(Name = "user_name", StringLength = 50)]
        public string UserName { get; set; }

        [Column(Name = "role")]
        public Role Role { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }

    }

    public enum Role
    {
        SuperAdmin = 0,//超管
        AppAdmin, // app管理员
        Editor, //
        Publisher //
    }

    public class Functions
    {
        public const string App_Add = "App_Add";
        public const string App_Edit = "App_Edit";
        public const string App_Delete = "App_Delete";

        public const string Config_Add = "Config_Add";
        public const string Config_Edit = "Config_Edit";
        public const string Config_Delete = "Config_Delete";

        public const string Config_Publish = "Config_Publish";
        public const string Config_Offline = "Config_Offline";

    }

    public class RoleFunctions
    {
        private Dictionary<Role, List<string>> _roleFunctions = new Dictionary<Role, List<string>>() {
            {
                Role.SuperAdmin, new List<string>{}
            },
            {
                Role.AppAdmin, new List<string>{}
            },
            {
                Role.Editor, new List<string>{}
            },
            {
                Role.Publisher, new List<string>{}
            }
        };
    }


}
