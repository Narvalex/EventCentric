using EasyTrade.Web.Api.DTOs;
using System.Web.Http;

namespace EasyTrade.Web.Api
{
    public class EmpresasApiController : ApiController
    {
        [HttpPost]
        [Route("api/empresas/nueva-empresa")]
        public IHttpActionResult NuevaEmpresa([FromBody]NuevaEmpresaDto empresa)
        {
            return this.Ok();
        }
    }
}
