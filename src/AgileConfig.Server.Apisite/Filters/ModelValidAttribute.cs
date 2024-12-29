using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgileConfig.Server.Apisite.Filters
{
    public class ModelVaildateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errMsg = new StringBuilder();
                foreach (var item in context.ModelState.Values)
                {
                    foreach (var error in item.Errors)
                    {
                        errMsg.Append(error.ErrorMessage + ";");
                    }
                }

                context.Result = new JsonResult(new {
                    success = false,
                    message = errMsg.ToString()
                });
            }
        }
    }
}
