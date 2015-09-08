using EasyTrade.EmpresasQueue.DTOs;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyTrade.EmpresasQueue.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EmpresasController : ApiController
    {
        [HttpPost]
        [Route("empresas/nueva-empresa")]
        public IHttpActionResult NuevaEmpresas([FromBody]NuevaEmpresaDto dto)
        {
            return this.Ok();
        }
    }
}
