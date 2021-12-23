using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_app_auth")]
    [OraclePrimaryKeyName("agc_user_app_auth_pk")]
    public class UserAppAuth
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "app_id", StringLength = 36)]
        public string AppId { get; set; }

        [Column(Name = "user_id", StringLength = 36)]
        public string UserId { get; set; }

        [Column(Name = "permission", StringLength = 50)]
        public string Permission { get; set; }
    }
}
