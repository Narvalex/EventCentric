using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace EventCentric.Authorization
{
    /// <summary>
    /// The authorization annotation.
    /// </summary>
    /// <remarks>
    /// More info on auth attribute: http://www.dotnetdreamer.net/aspnet-web-api-custom-authorize-attribute-with-custom-database-table
    /// Extracting the token: http://stackoverflow.com/questions/14967457/how-to-extract-custom-header-value-in-web-api-message-handler
    /// </remarks>
    public class AuthorizationRequiredAttribute : AuthorizeAttribute
    {
        private readonly IIdentity identity;

        public AuthorizationRequiredAttribute()
        {
            this.identity = Identity.Resolve();
        }

        private string responseReason = string.Empty;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (IsApiPageRequested(actionContext))
            {
                if (false)
                {
                    // logic that will run before even authorizing the customer / user. if this logic fails
                    // then the user checking against our custom database table will not processed.
                    // you can skip this if you don't have such requirements and directly call

                    //this.HandleUnauthorizedRequest(actionContext);
                    //this.responseReason = "Web services plugin is not available in this store";
                }
                else
                    base.OnAuthorization(actionContext);
            }
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // logic for check whether we have an attribute with ByPassAuthorization = true e.g [ByPassAuthorization(true)], if so then just return true 
            //  E.G.
            // if (ByPassAuthorization
            //    || GetApiAuthorizeAttributes(actionContext.ActionDescriptor).Any(x => x.ByPassAuthorization))
            //    return true;
            if (this.GetAuthAttributes<AllowAnonymousAttribute>(actionContext.ActionDescriptor).Any())
                return true;

            IEnumerable<string> values;
            if (actionContext.Request.Headers.TryGetValues("Authorization", out values))
            {
                if (values.Any(v => this.identity.IsAuthorized(v)))
                    return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
            if (!string.IsNullOrEmpty(this.responseReason))
                actionContext.Response.ReasonPhrase = responseReason;
        }

        private IEnumerable<T> GetAuthAttributes<T>(HttpActionDescriptor descriptor) where T : class
        {
            return descriptor.GetCustomAttributes<T>(true)
                    .Concat(descriptor.ControllerDescriptor.GetCustomAttributes<T>(true));
        }

        bool IsApiPageRequested(HttpActionContext actionContext)
        {
            var apiAttributes = this.GetAuthAttributes<AuthorizationRequiredAttribute>(actionContext.ActionDescriptor);
            if (apiAttributes != null && apiAttributes.Any())
                return true;
            return false;
        }
    }
}
