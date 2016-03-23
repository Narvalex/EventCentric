using Occ.ServiceHost.App_Start;
using Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]
namespace Occ.ServiceHost.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
