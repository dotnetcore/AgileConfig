using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgileConfig.Server.Apisite.Models;
using AgileConfig.Server.Apisite.Websocket;
using AgileConfig.Server.Common;
using AgileConfig.Server.IService;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Login(string password)
        {
            return null;
        }

        public IActionResult InitPassword(string password)
        {
            return null;
        }
   
    }
}
