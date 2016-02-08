using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http.Controllers
{
    public static class ActionContextExtensions
    {
        /// <summary>
        /// Usability overload that is usefull to retrieve the token of the current user.
        /// </summary>
        public static string GetToken(this HttpActionContext context)
        {
            IEnumerable<string> values;
            return context.Request.Headers.TryGetValues("Authorization", out values) ? values.First() : null;
        }
    }
}