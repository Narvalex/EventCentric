using System;
using System.Web.Http;

namespace Clientes.Client.Controllers
{
    public class ClientesController : ApiController
    {
        private readonly ITelefonicaAdminClient app;

        public ClientesController(ITelefonicaAdminClient app)
        {
            this.app = app;
        }

        [HttpGet]
        [Route("SolicitudNuevoClienteRecibida/{nombre}")]
        public IHttpActionResult SolicitudNuevoClienteRecibida(string nombre)
        {
            app.SolicitudNuevoClienteRecibida(nombre);
            return this.Ok();
        }

        [HttpGet]
        [Route("ClienteAbonoPorSaldo/{idCliente}/{montoAbonado}")]
        public IHttpActionResult ClienteAbonoPorSaldo(Guid idCliente, int montoAbonado)
        {
            app.ClienteAbonoPorSaldo(idCliente, montoAbonado);
            return this.Ok();
        }

        [HttpGet]
        [Route("ClienteGastoSaldo/{idCliente}/{saldoGastado}")]
        public IHttpActionResult ClienteGastoSaldo(Guid idCliente, int saldoGastado)
        {
            app.ClienteGastoSaldo(idCliente, saldoGastado);
            return this.Ok();
        }
    }
}
