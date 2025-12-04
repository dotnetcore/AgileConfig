using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

[Table(Name = "agc_config_published")]
[OraclePrimaryKeyName("agc_config_published_pk")]
public class ConfigPublished : IEntity<string>
{
    [Column(Name = "app_id", StringLength = 36)]
    public string AppId { get; set; }

    [Column(Name = "g", StringLength = 100)]
    public string Group { get; set; }

    [Column(Name = "k", StringLength = 100)]
    public string Key { get; set; }

    public string Value { get; set; }

    [Column(Name = "publish_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? PublishTime { get; set; }

    [Column(Name = "config_id", StringLength = 36)]
    public string ConfigId { get; set; }

    [Column(Name = "publish_timeline_id", StringLength = 36)]
    public string PublishTimelineId { get; set; }

    [Column(Name = "version")] public int Version { get; set; }

    [Column(Name = "status")] public ConfigStatus Status { get; set; }

    [Column(Name = "env", StringLength = 50)]
    public string Env { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }
}

public static class ConfigPublishedExt
{
    public static Config Convert(this ConfigPublished configPublished)
    {
        return new Config
        {
            Id = configPublished.ConfigId,
            Group = configPublished.Group,
            Key = configPublished.Key,
            Value = configPublished.Value
        };
    }
}