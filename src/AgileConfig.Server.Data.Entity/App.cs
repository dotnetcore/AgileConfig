using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

public interface IAppModel
{
    string Id { get; set; }

    string Name { get; set; }

    string Group { get; set; }

    string Creator { get; set; }

    DateTime CreateTime { get; set; }
}

public enum AppType
{
    PRIVATE,
    Inheritance
}

[Table(Name = "agc_app")]
[OraclePrimaryKeyName("agc_app_pk")]
public class App : IAppModel, IEntity<string>
{
    [Column(Name = "secret", StringLength = 36)]
    public string Secret { get; set; }

    [Column(Name = "update_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? UpdateTime { get; set; }

    [Column(Name = "enabled")] public bool Enabled { get; set; }

    [Column(Name = "type")] public AppType Type { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }

    [Column(Name = "name", StringLength = 50)]
    public string Name { get; set; }

    [Column(Name = "group", StringLength = 50)]
    public string Group { get; set; }

    [Column(Name = "creator", StringLength = 36)]
    public string Creator { get; set; }

    [Column(Name = "create_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }
}
