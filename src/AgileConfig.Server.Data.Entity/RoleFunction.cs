using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

[Table(Name = "agc_role_function")]
[OraclePrimaryKeyName("agc_role_function_pk")]
public class RoleFunction : IEntity<string>
{
    [Column(Name = "role_id", StringLength = 64)]
    public string RoleId { get; set; }

    [Column(Name = "function_id", StringLength = 64)]
    public string FunctionId { get; set; }

    [Column(Name = "create_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }
}