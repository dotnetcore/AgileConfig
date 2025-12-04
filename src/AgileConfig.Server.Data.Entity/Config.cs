using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

/// <summary>
///     Deleted = 0,
///     Enabled = 1,
/// </summary>
public enum ConfigStatus
{
    Deleted = 0,
    Enabled = 1
}

/// <summary>
///     Add = 0,
///     Edit = 1,
///     Deleted = 2,
///     Commit = 10
/// </summary>
public enum EditStatus
{
    Add = 0,
    Edit = 1,
    Deleted = 2,
    Commit = 10
}

/// <summary>
///     WaitPublish = 0,
///     Online = 1,
/// </summary>
public enum OnlineStatus
{
    WaitPublish = 0,
    Online = 1
}

[Table(Name = "agc_config")]
[OraclePrimaryKeyName("agc_config_pk")]
public class Config : IEntity<string>
{
    [Column(Name = "app_id", StringLength = 36)]
    public string AppId { get; set; }

    [Column(Name = "g", StringLength = 100)]
    public string Group { get; set; }

    [Column(Name = "k", StringLength = 100)]
    public string Key { get; set; }

    public string Value { get; set; }

    [Column(Name = "description", StringLength = 200)]
    public string Description { get; set; }

    [Column(Name = "create_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }

    [Column(Name = "update_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? UpdateTime { get; set; }

    [Column(Name = "status")] public ConfigStatus Status { get; set; }

    [Column(Name = "online_status")] public OnlineStatus OnlineStatus { get; set; }

    [Column(Name = "edit_status")] public EditStatus EditStatus { get; set; }

    [Column(Name = "env", StringLength = 50)]
    public string Env { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }
}