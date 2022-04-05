using System;
using AgileConfig.Server.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace AgileConfig.Server.Apisite.Models;

public class ServiceInfoVM
{
    public string Id { get; set; }

    [Required(ErrorMessage = "服务Id不能为空")]
    [MaxLength(100, ErrorMessage = "服务Id长度不能超过100")]
    public string ServiceId { get; set; }
    
    [Required(ErrorMessage = "服务名不能为空")]
    [MaxLength(100, ErrorMessage = "服务名长度不能超过100")]
    public string ServiceName { get; set; }
    
    [MaxLength(100, ErrorMessage = "IP长度不能超过100")]
    public string Ip { get; set; }

    public int? Port { get; set; }

    public string MetaData { get; set; }

    public ServiceStatus Status { get; set; }

    public DateTime? RegisterTime { get; set; }

    public DateTime? LastHeartBeat { get; set; }
    
    [Required(ErrorMessage = "健康检测模式不能为空")]
    [MaxLength(10, ErrorMessage = "健康检测模式长度不能超过10位")]
    public string HeartBeatMode { get; set; }
    
    [MaxLength(2000, ErrorMessage = "检测URL长度不能超过2000")]
    public string CheckUrl { get; set; }

    [MaxLength(2000, ErrorMessage = "告警URL长度不能超过2000")]
    public string AlarmUrl { get; set; }
}