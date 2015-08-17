using System.Web.Http;

namespace Clientes.CommandProcessor.Api
{
    public class ClienteController : ApiController
    {
        [HttpGet]
        [Route("api/cliente/agregarCliente/{nombre}")]
        public IHttpActionResult AgregarCliente(string nombre)
        {
            return this.Ok();
        }

        [HttpGet]
        [Route("api/cliente/agregarSaldo/{saldo}")]
        public IHttpActionResult AgregarSaldo(int saldo)
        {
            return this.Ok();
        }

        [HttpGet]
        [Route("api/cliente/quitarSaldo/{saldo}")]
        public IHttpActionResult QuitarSaldo(int saldo)
        {
            return this.Ok();
        }
    }
}
