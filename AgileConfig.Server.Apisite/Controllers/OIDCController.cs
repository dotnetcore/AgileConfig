using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileConfig.Server.Apisite.Controllers
{
    public class OIDCController : Controller
    {
        /// <summary>
        /// pass the oidc code to frontend
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Index(string code)
        {
            return Redirect("http://localhost:8000/ui/#/oidc/login?code=" + code);
        }
    }
}
