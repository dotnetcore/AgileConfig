using AgileConfig.Server.Common;
using FreeSql.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AgileConfig.Server.Data.Entity;

/// <summary>
///     Represents the inheritance relationship between applications.
/// </summary>
[Table(Name = "agc_appInheritanced")]
[OraclePrimaryKeyName("agc_appInheritanced_pk")]
[BsonIgnoreExtraElements]
public class AppInheritanced : IEntity<string>
{
    [Column(Name = "appid", StringLength = 36)]
    public string AppId { get; set; }

    [Column(Name = "inheritanced_appid", StringLength = 36)]
    public string InheritancedAppId { get; set; }

    [Column(Name = "sort")] public int Sort { get; set; }

    [Column(Name = "id", StringLength = 36)]
    public string Id { get; set; }
}