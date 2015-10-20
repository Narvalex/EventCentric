using InformesDeServicio.Publicadores.Server.App_Start;
using Owin;

[assembly: Microsoft.Owin.OwinStartup(typeof(Startup))]
namespace InformesDeServicio.Publicadores.Server.App_Start
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }

}
