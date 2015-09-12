using System;
using System.Web.Http;

namespace EasyTrade.EmpresasReadModel.Web.Controllers
{
    public class DaoController : ApiController
    {
        [HttpGet]
        [Route("dao/await-nueva-empresa/{transactionId}")]
        public IHttpActionResult AwaitNuevaEmpresa(Guid transactionId)
        {
            return this.Ok(transactionId.ToString());
        }
    }
}
