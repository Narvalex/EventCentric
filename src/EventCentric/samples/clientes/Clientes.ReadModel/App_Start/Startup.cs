using Owin;

[assembly: Microsoft.Owin.OwinStartupAttribute(typeof(EventCentric.Web.Startup))]
namespace EventCentric.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}