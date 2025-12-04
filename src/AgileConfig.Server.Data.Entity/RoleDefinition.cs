using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

[Table(Name = "agc_role")]
[OraclePrimaryKeyName("agc_role_pk")]
public class Role : IEntity<string>
{
    [Column(Name = "name", StringLength = 128)]
    public string Name { get; set; }

    [Column(Name = "description", StringLength = 512)]
    public string Description { get; set; }

    [Column(Name = "is_system")] public bool IsSystem { get; set; }

    [Column(Name = "create_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }

    [Column(Name = "update_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? UpdateTime { get; set; }

    [Column(Name = "id", StringLength = 64)]
    public string Id { get; set; }
}