using AgileConfig.Server.Apisite.Controllers.api.Models;
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
    /// <summary>
    /// 注册中心接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterCenterController : Controller
    {
        private readonly IAppService _appService;
        public RegisterCenterController(IAppService appService)
        {
            _appService = appService;
        }
       
        [HttpPost]
        public object Register([FromBody]RegisterServiceInfoVM model)
        {
            return new
            {
                uniqueId = Guid.NewGuid(),
            };
        }


        [HttpDelete("{id}")]
        public object UnRegister(string id)
        {
            return new
            {
                uniqueId = id,
            };
        }
    }
}
