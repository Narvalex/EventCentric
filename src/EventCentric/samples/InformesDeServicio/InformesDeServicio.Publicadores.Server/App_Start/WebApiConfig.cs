using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace InformesDeServicio.Publicadores.Server.App_Start
{
    public static class WebApiConfig
    {
        /// <summary>
        /// Register Web API configuration ans services.
        /// </summary>
        /// <param name="config">An <see cref="HttpConfiguration"/> instance.</param>
        public static void Register(HttpConfiguration config)
        {
            // More info: http://www.asp.net/web-api/overview/security/enabling-cross-origin-requests-in-web-api
            config.EnableCors();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
