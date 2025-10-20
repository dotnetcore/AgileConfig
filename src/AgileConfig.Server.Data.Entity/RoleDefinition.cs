using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AgileConfig.Server.Data.Entity
{
    [Table(Name = "agc_role_definition")]
    [OraclePrimaryKeyName("agc_role_definition_pk")]
    public class RoleDefinition : IEntity<string>
    {
        [Column(Name = "id", StringLength = 64)]
        public string Id { get; set; }

        [Column(Name = "code", StringLength = 64)]
        public string Code { get; set; }

        [Column(Name = "name", StringLength = 128)]
        public string Name { get; set; }

        [Column(Name = "description", StringLength = 512)]
        public string Description { get; set; }

        [Column(Name = "is_system")]
        public bool IsSystem { get; set; }

        [Column(Name = "functions", StringLength = -1)]
        public string FunctionsJson { get; set; }

        [Column(Name = "create_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdateTime { get; set; }
    }
}
