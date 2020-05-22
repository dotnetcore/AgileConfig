using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    [Authorize]
    public class SysLogController : Controller
    {
        private readonly ISysLogService _sysLogService;

        public SysLogController(ISysLogService sysLogService)
        {
            _sysLogService = sysLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string appId, DateTime startTime, DateTime endTime, int pageSize, int pageIndex)
        {
            var pageList = await _sysLogService.SearchPage(appId, startTime, endTime.Date.AddDays(1), pageSize, pageIndex);

            return Json(new
            {
                success = true,
                data = pageList
            });
        }
    }
}
