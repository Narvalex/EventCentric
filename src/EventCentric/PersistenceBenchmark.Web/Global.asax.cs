using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace PersistenceBenchmark.Web
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
            });

            Task.Factory.StartNewLongRunning(() => BenchmarkRunner.RunAsConfigured(UnityConfig.GetConfiguredContainer(false)));
        }

        protected void Application_End(object sender, EventArgs e)
        {
            //DbManager.DropDb();
        }
    }
}