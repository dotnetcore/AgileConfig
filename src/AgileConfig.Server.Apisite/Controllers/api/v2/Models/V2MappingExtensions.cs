using System;
using System.Collections.Generic;
using AgileConfig.Server.Data.Entity;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Controllers.api.v2.Models;

internal static class V2MappingExtensions
{
    public static ApplicationResource ToV2Resource(this App app, IReadOnlyList<string> inheritsFrom)
    {
        return new ApplicationResource
        {
            Id = app.Id,
            Name = app.Name,
            Group = app.Group,
            Enabled = app.Enabled,
            IsInheritanceSource = app.Type == AppType.Inheritance,
            InheritsFrom = inheritsFrom,
            Creator = app.Creator,
            CreatedAt = app.CreateTime,
            UpdatedAt = app.UpdateTime
        };
    }

    public static ConfigurationResource ToV2Resource(this Config config)
    {
        return new ConfigurationResource
        {
            Id = config.Id,
            ApplicationId = config.AppId,
            Environment = config.Env,
            Group = config.Group,
            Key = config.Key,
            Value = config.Value,
            Description = config.Description,
            Status = config.Status.ToString(),
            PublicationStatus = config.OnlineStatus.ToString(),
            ChangeType = config.EditStatus.ToString(),
            CreatedAt = config.CreateTime,
            UpdatedAt = config.UpdateTime
        };
    }

    public static ReleaseResource ToV2Resource(this PublishTimeline timeline)
    {
        return new ReleaseResource
        {
            Id = timeline.Id,
            ApplicationId = timeline.AppId,
            Environment = timeline.Env,
            Version = timeline.Version,
            Log = timeline.Log,
            PublishedBy = timeline.PublishUserName ?? timeline.PublishUserId,
            PublishedAt = timeline.PublishTime
        };
    }

    public static NodeResource ToV2Resource(this ServerNode node)
    {
        return new NodeResource
        {
            Id = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(node.Id)),
            Address = node.Id,
            Remark = node.Remark,
            Status = node.Status.ToString(),
            LastEchoAt = node.LastEchoTime
        };
    }

    public static string DecodeNodeId(string id)
    {
        try
        {
            return System.Text.Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(id));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public static ServiceInstanceResource ToV2Resource(this ServiceInfo instance)
    {
        var metadata = new List<string>();
        try
        {
            metadata = JsonConvert.DeserializeObject<List<string>>(instance.MetaData) ?? [];
        }
        catch (JsonException)
        {
        }

        return new ServiceInstanceResource
        {
            Id = instance.Id,
            ServiceId = instance.ServiceId,
            Name = instance.ServiceName,
            IpAddress = instance.Ip,
            Port = instance.Port,
            Metadata = metadata,
            Status = instance.Status.ToString(),
            HeartbeatMode = instance.HeartBeatMode,
            HealthCheckUrl = instance.CheckUrl,
            AlarmUrl = instance.AlarmUrl,
            RegisteredAt = instance.RegisterTime,
            LastHeartbeatAt = instance.LastHeartBeat
        };
    }
}
