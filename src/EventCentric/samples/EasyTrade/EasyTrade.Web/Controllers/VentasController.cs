using System.Web.Mvc;

namespace EasyTrade.Web.Controllers
{
    public class VentasController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }
    }
}