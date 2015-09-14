using EasyTrade.EmpresasReadModel.Dao;
using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EasyTrade.EmpresasReadModel.Web.Controllers
{
    [EnableCors("*", "*", "*")]
    public class DaoController : ApiController
    {
        private readonly IEmpresasReadModelDao dao;

        public DaoController(IEmpresasReadModelDao dao)
        {
            this.dao = dao;
        }

        [HttpGet]
        [Route("dao/await-result/{transactionId}")]
        public IHttpActionResult AwaitResult(Guid transactionId)
        {
            var result = this.dao.AwaitResult(transactionId);
            return this.Ok(result);
        }

        [HttpGet]
        [Route("dao/obtener-todas-las-empresas")]
        public IHttpActionResult ObtenerTodasLasEmpresas()
        {
            var empresas = this.dao.ObtenerListaDeTodasLasEmpresas();
            return this.Ok(empresas);
        }
    }
}
