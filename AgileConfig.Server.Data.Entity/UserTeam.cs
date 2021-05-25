using FreeSql.DataAnnotations;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_user_team")]
    public class UserTeam
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "user_id", StringLength = 50)]
        public string UserId { get; set; }

        [Column(Name = "team_id", StringLength = 36)]
        public string TeamId { get; set; }

        [Column(Name = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
