using InformesDeServicio.Publicadores.Server.App_Start;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
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
