using AgileConfig.Server.Apisite.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite.Controllers.api
{
    [TypeFilter(typeof(AppBasicAuthenticationAttribute))]
    [Route("api/[controller]")]
    public class NodeController : Controller
    {
    }
}
