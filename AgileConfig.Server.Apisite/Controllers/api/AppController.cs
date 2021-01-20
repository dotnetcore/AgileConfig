using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    [TypeFilter(typeof(AdmBasicAuthenticationAttribute))]
    [Route("api/[controller]")]
    public class AppController : Controller
    {
        private readonly IAppService _appService;
        private readonly ISysLogService _sysLogService;

        public AppController(IAppService appService, ISysLogService sysLogService)
        {
            _appService = appService;
            _sysLogService = sysLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var apps = await _appService.GetAllAppsAsync();
            var vms = apps.Select(x => {
                return new AppVM
                {
                    Id = x.Id,
                    Name = x.Name,
                    Secret = x.Secret,
                    Inheritanced = x.Type == AppType.Inheritance,
                    Enabled = x.Enabled,
                };
            });

            return Json(vms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var ctrl = new Controllers.AppController(
             _appService,
             _sysLogService
             );
            var result = (await ctrl.Get(id)) as JsonResult;
            dynamic obj = result.Value;

            if (obj.success)
            {
                AppVM appVM = obj.data;
                return Json(new
                {
                    appVM.Id,
                    appVM.Name,
                    appVM.Secret,
                    appVM.Inheritanced,
                    appVM.Enabled,
                    inheritancedApps = appVM.inheritancedApps.Select(x =>new { 
                        x.Id
                    })
                }) ;
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AppVM model)
        {
            var requiredResult = CheckRequired(model);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = requiredResult.Item2
                });
            }

            var ctrl = new Controllers.AppController(
                _appService,
                _sysLogService
                );

            var result = (await ctrl.Add(model)) as JsonResult;

            dynamic obj = result.Value;

            if (obj.success == true)
            {
                return Created("/api/app/" + obj.data.Id, "");
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] AppVM model)
        {
            var requiredResult = CheckRequired(model);

            if (!requiredResult.Item1)
            {
                Response.StatusCode = 400;
                return Json(new
                {
                    message = requiredResult.Item2
                });
            }

            var ctrl = new Controllers.AppController(
                  _appService,
                  _sysLogService
                  );

            model.Id = id;
            var result = (await ctrl.Edit(model)) as JsonResult;

            dynamic obj = result.Value;
            if (obj.success == true)
            {
                return Ok();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var ctrl = new Controllers.AppController(
                    _appService,
                    _sysLogService
                    );

            var result = (await ctrl.Delete(id)) as JsonResult;

            dynamic obj = result.Value;
            if (obj.success == true)
            {
                return NoContent();
            }

            Response.StatusCode = 400;
            return Json(new
            {
                obj.message
            });
        }

        private (bool, string) CheckRequired(AppVM model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return (false, "Id不能为空");
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return (false, "Name不能为空");
            }

            return (true, "");
        }
    }
}
