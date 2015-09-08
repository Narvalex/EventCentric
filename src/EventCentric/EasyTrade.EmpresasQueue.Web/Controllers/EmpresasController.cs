using System.Web.Mvc;

namespace EasyTrade.EmpresasQueue.Web.Controllers
{
    public class EmpresasController : Controller
    {
        // GET: Empresas
        [HttpPost]
        [Route("empresas/nueva-empresa")]
        public ActionResult NuevaEmpresa(NuevaEmpresaDto empresa)
        {
            return this.Content("Hola");
        }
    }
    public class NuevaEmpresaDto
    {
        public string Nombre { get; set; }
        public string RUC { get; set; }
        public string Descripcion { get; set; }
    }

}