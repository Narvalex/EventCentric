using Newtonsoft.Json.Serialization;
using Occ.ServiceHost.App_Start;
using System;
using System.Web.Http;

namespace Occ.ServiceHost
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(config =>
            {
                // More info: http://www.asp.net/web-api/overview/security/enabling-cross-origin-requests-in-web-api
                config.EnableCors();

                config.MapHttpAttributeRoutes();

                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional });

                var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
                jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                var container = UnityConfig.GetConfiguredContainer();
            });
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}