using EasyTrade.EmpresasQueue.DTOs;
using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyTrade.EmpresasQueue.Web.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EmpresasController : ApiController
    {
        private readonly IEmpresasQueueApp app;

        public EmpresasController(IEmpresasQueueApp app)
        {
            this.app = app;
        }

        [HttpPost]
        [Route("empresas/nueva-empresa")]
        public IHttpActionResult NuevaEmpresa([FromBody]NuevaEmpresaDto dto)
        {
            var transactionId = this.app.NuevaEmpresa(dto);
            return this.Ok(transactionId);
        }

        [HttpPost]
        [Route("empresas/desactivar-empresa/{idEmpresa}")]
        public IHttpActionResult DesactivarEmpresa([FromUri]Guid idEmpresa)
        {
            var transactionId = this.app.DesactivarEmpresa(idEmpresa);
            return this.Ok(transactionId);
        }
    }
}
