using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Models;

public class AppExportRequest
{
    [JsonProperty("appIds")]
    public List<string> AppIds { get; set; }
}

public class AppImportRequest
{
    [JsonProperty("file")]
    public AppExportFileVM File { get; set; }
}

public class AppExportFileVM
{
    [JsonProperty("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonProperty("exportedAt")]
    public DateTime ExportedAt { get; set; }

    [JsonProperty("apps")]
    public List<AppExportItemVM> Apps { get; set; } = new();
}

public class AppExportItemVM
{
    [JsonProperty("app")]
    public AppExportAppVM App { get; set; }

    [JsonProperty("envs")]
    public Dictionary<string, List<AppExportConfigVM>> Envs { get; set; } = new();
}

public class AppExportAppVM
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("group")]
    public string Group { get; set; }

    [JsonProperty("secret")]
    public string Secret { get; set; }

    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("inheritanced")]
    public bool Inheritanced { get; set; }

    [JsonProperty("inheritancedApps")]
    public List<string> InheritancedApps { get; set; } = new();
}

public class AppExportConfigVM
{
    [JsonProperty("group")]
    public string Group { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}

public class AppImportPreviewVM
{
    [JsonProperty("apps")]
    public List<AppImportPreviewItemVM> Apps { get; set; } = new();

    [JsonProperty("errors")]
    public List<string> Errors { get; set; } = new();
}

public class AppImportPreviewItemVM
{
    [JsonProperty("appId")]
    public string AppId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("group")]
    public string Group { get; set; }

    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("inheritanced")]
    public bool Inheritanced { get; set; }

    [JsonProperty("inheritancedApps")]
    public List<string> InheritancedApps { get; set; } = new();

    [JsonProperty("envCount")]
    public int EnvCount { get; set; }

    [JsonProperty("configCount")]
    public int ConfigCount { get; set; }

    [JsonProperty("order")]
    public int Order { get; set; }
}
