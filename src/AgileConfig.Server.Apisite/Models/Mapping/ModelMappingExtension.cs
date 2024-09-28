using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.Apisite.Controllers.api.Models;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Models.Mapping
{
    /// <summary>
    /// Do not ask me why not use AutoMapper, I don't know. Just like manual mapping, it's simple and clear.
    /// </summary>
    public static class AppExtension
    {
        public static AppVM ToAppVM(this App app)
        {
            if (app == null)
            {
                return null;
            }

            var appVM = new AppVM
            {
                Id = app.Id,
                Name = app.Name,
                Group = app.Group,
                Secret = app.Secret,
                Enabled = app.Enabled,
                Inheritanced = app.Type == AppType.Inheritance,
                AppAdmin = app.AppAdmin,
                CreateTime = app.CreateTime,
            };

            return appVM;
        }

        public static AppListVM ToAppListVM(this App app)
        {
            if (app == null)
            {
                return null;
            }

            var vm = new AppListVM
            {
                Id = app.Id,
                Name = app.Name,
                Group = app.Group,
                Secret = app.Secret,
                Inheritanced = app.Type == AppType.Inheritance,
                Enabled = app.Enabled,
                UpdateTime = app.UpdateTime,
                CreateTime = app.CreateTime,
                AppAdmin = app.AppAdmin,
            };

            return vm;
        }

        public static ApiAppVM ToApiAppVM(this App vm)
        {
            if (vm == null)
            {
                return null;
            }

            return new ApiAppVM
            {
                Id = vm.Id,
                Name = vm.Name,
                Secret = vm.Secret,
                Inheritanced = vm.Type == AppType.Inheritance,
                Enabled = vm.Enabled,
                AppAdmin = vm.AppAdmin,
                Group = vm.Group,
                CreateTime = vm.CreateTime
            };
        }
    }

    public static class PublishTimelineExtension
    {
        public static ApiPublishTimelineVM ToApiPublishTimelimeVM(this PublishTimeline timeline)
        {
            if (timeline == null)
            {
                return null;
            }

            return new ApiPublishTimelineVM
            {
                Id = timeline.Id,
                Version = timeline.Version,
                AppId = timeline.AppId,
                Log = timeline.Log,
                PublishTime = timeline.PublishTime,
                PublishUserId = timeline.PublishUserId,
                Env = timeline.Env
            };
        }
    }

    public static class ServerNodeExtension
    {
        public static ApiNodeVM ToApiNodeVM(this ServerNode node)
        {
            if (node == null)
            {
                return null;
            }

            return new  ApiNodeVM
            {
                Address = node.Id,
                Remark = node.Remark,
                LastEchoTime = node.LastEchoTime,
                Status = node.Status
            };
        }
    }

    public static class ServiceInfoExtension
    {
        public static ApiServiceInfoVM ToApiServiceInfoVM(this ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return null;
            }

            var vm = new ApiServiceInfoVM
            {
                ServiceId = serviceInfo.ServiceId,
                ServiceName = serviceInfo.ServiceName,
                Ip = serviceInfo.Ip,
                Port = serviceInfo.Port,
                MetaData = new List<string>(),
                Status = serviceInfo.Status
            };

            try
            {
                vm.MetaData = JsonConvert.DeserializeObject<List<string>>(serviceInfo.MetaData);
            }
            catch
            {
            }

            return vm;
        }
    }

}
