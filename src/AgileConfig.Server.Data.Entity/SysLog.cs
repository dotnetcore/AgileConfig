using System;
using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

public enum SysLogType
{
    Normal = 0,
    Warn = 1
}

[Table(Name = "agc_sys_log")]
[OraclePrimaryKeyName("agc_sys_log_pk")]
public class SysLog : IEntity<string>
{
    public SysLog()
    {
        Id = Guid.NewGuid().ToString("N");
    }

    [Column(Name = "app_id", StringLength = 36)]
    public string AppId { get; set; } = "";

    [Column(Name = "log_type")] public SysLogType LogType { get; set; }

    [Column(Name = "log_time")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? LogTime { get; set; }

    [Column(Name = "log_text", StringLength = 2000)]
    public string LogText { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }

    public override string ToString()
    {
        return $"Id:{Id}, AppId:{AppId}, LogType:{LogType}, LogTime:{LogTime}, LogText:{LogText}";
    }
}