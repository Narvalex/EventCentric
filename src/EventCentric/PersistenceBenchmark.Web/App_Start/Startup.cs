using Owin;
using PersistenceBenchmark.Web.App_Start;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]
namespace PersistenceBenchmark.Web.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
