using System;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Filters;
using AgileConfig.Server.Data.Entity;
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
        public async Task<IActionResult> Search(string appId, SysLogType? logType, DateTime? startTime, DateTime? endTime, int current = 1, int pageSize = 20)
        {
            if (current <= 0)
            {
                throw new ArgumentException("current can not less than 1 .");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("pageSize can not less than 1 .");
            }

            var pageList = await _sysLogService.SearchPage(appId, logType, startTime, endTime?.Date.AddDays(1), pageSize, current);
            var total = await _sysLogService.Count(appId, logType, startTime, endTime?.Date.AddDays(1));
            var totalPages = total / pageSize;
            if ((total % pageSize) > 0)
            {
                totalPages++;
            }

            return Json(new
            {
                current,
                pageSize,
                success = true,
                total = total,
                data = pageList
            });
        }
    }
}
