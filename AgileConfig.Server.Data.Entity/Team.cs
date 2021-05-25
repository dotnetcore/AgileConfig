using FreeSql.DataAnnotations;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_team")]
    public class Team
    {
        [Column(Name = "id", StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "name", StringLength = 50)]
        public string Name { get; set; }
    }
}
