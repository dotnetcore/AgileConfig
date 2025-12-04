using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

[Table(Name = "agc_function")]
[OraclePrimaryKeyName("agc_function_pk")]
public class Function : IEntity<string>
{
    [Column(Name = "code", StringLength = 64)]
    public string Code { get; set; }

    [Column(Name = "name", StringLength = 128)]
    public string Name { get; set; }

    [Column(Name = "description", StringLength = 512)]
    public string Description { get; set; }

    [Column(Name = "category", StringLength = 64)]
    public string Category { get; set; }

    [Column(Name = "sort_index")] public int SortIndex { get; set; }

    [Column(Name = "create_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }

    [Column(Name = "update_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? UpdateTime { get; set; }

    [Column(Name = "id", StringLength = 64)]
    public string Id { get; set; }
}