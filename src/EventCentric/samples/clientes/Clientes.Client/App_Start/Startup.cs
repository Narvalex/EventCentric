using Owin;

[assembly: Microsoft.Owin.OwinStartupAttribute(typeof(Journey.Web.Startup))]
namespace Journey.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}