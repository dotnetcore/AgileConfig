using System;
using System.Threading.Tasks;
using System.Linq;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using AgileConfig.Server.Apisite.Websocket;
using Newtonsoft.Json;

namespace AgileConfig.Server.Apisite.Controllers
{
    [ModelVaildate]
    public class ConfigController : Controller
    {
        private readonly IConfigService _configService;
        public ConfigController(IConfigService configService)
        {
            _configService = configService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]ConfigVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var oldConfig = await _configService.GetByAppIdKey(model.AppId, model.Group, model.Key);
            if (oldConfig != null)
            {

                return Json(new
                {
                    success = false,
                    message = "配置键已存在，请重新输入。"
                });
            }

            var config = new Config();
            config.Id = Guid.NewGuid().ToString("N");
            config.Key = model.Key;
            config.AppId = model.AppId;
            config.Description = model.Description;
            config.Value = model.Value;
            config.Group = model.Group;
            config.Status = ConfigStatus.Enabled;
            config.CreateTime = DateTime.Now;
            config.UpdateTime = null;

            var result = await _configService.AddAsync(config);

            if (result)
            {
                //notice clients
                var msg = new { 
                    Action="add",
                    Node = new
                    {
                        group = config.Group,
                        key = config.Key,
                        value = config.Value
                    }
                };
                var json = JsonConvert.SerializeObject(msg);
                WebsocketCollection.Instance.SendToAll(json);
            }

            return Json(new
            {
                success = result,
                message = !result ? "新建配置失败，请查看错误日志" : ""
            });
        }


        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]ConfigVM model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var config = await _configService.GetAsync(model.Id);
            var oldConfig = await _configService.GetAsync(model.Id);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            if (config.Group != model.Group || config.Key != model.Key)
            {
                var anotherConfig = await _configService.GetByAppIdKey(model.AppId, model.Group, model.Key);
                if (anotherConfig != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "配置键已存在，请重新输入。"
                    });
                }
            }

            config.AppId = model.AppId;
            config.Description = model.Description;
            config.Key = model.Key;
            config.Value = model.Value;
            config.Group = model.Group;
            config.Status = model.Status;
            config.UpdateTime = DateTime.Now;

            var result = await _configService.UpdateAsync(config);

            if (result)
            {
                //notice clients
                var msg = new
                {
                    Action = "update",
                    OldNode = new {
                        group = oldConfig.Group,
                        key = oldConfig.Key,
                        value = oldConfig.Value
                    },
                    Node = new
                    {
                        group = config.Group,
                        key = config.Key,
                        value = config.Value
                    }
                };
                var json = JsonConvert.SerializeObject(msg);
                WebsocketCollection.Instance.SendToAll(json);
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改配置失败，请查看错误日志。" : ""
            });
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var configs = await _configService.GetAllConfigsAsync();

            return Json(new
            {
                success = true,
                data = configs
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string appId, string group, string key)
        {
            var configs = await _configService.Search(appId, group, key);
            configs = configs.Where(c => c.Status == ConfigStatus.Enabled)
                .OrderBy(c => c.AppId).ThenBy(c => c.Group).ThenBy(c => c.Key)
                .ToList();

            return Json(new
            {
                success = true,
                data = configs
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var config = await _configService.GetAsync(id);

            return Json(new
            {
                success = config != null,
                data = config,
                message = config == null ? "未找到对应的配置项。" : ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var config = await _configService.GetAsync(id);
            if (config == null)
            {
                return Json(new
                {
                    success = false,
                    message = "未找到对应的配置项。"
                });
            }

            config.Status = ConfigStatus.Deleted;

            var result = await _configService.UpdateAsync(config);

            if (result)
            {
                //notice clients
                var msg = new
                {
                    Action = "remove",
                    Node = new
                    {
                        group = config.Group,
                        key = config.Key,
                        value = config.Value
                    }
                };
                var json = JsonConvert.SerializeObject(msg);
                WebsocketCollection.Instance.SendToAll(json);
            }

            return Json(new
            {
                success = result,
                message = !result ? "修改配置失败，请查看错误日志" : ""
            });
        }
    }
}
