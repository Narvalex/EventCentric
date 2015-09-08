using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EasyTrade.Common
{
    public class EnableCorsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response != null)
                actionExecutedContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            base.OnActionExecuted(actionExecutedContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Response != null)
                actionContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            base.OnActionExecuting(actionContext);
        }
    }
}
