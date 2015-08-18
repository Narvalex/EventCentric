using Clientes.CommandProcessor.Commands;
using Clientes.CommandProcessor.Processor;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;
using System.Web.Http;

namespace Clientes.CommandProcessor.Api
{
    public class ClienteController : ApiController
    {
        private readonly IClientBus bus;

        public ClienteController(IClientBus bus)
        {
            this.bus = bus;
        }

        [HttpGet]
        [Route("api/cliente/agregarCliente/{nombre}")]
        public IHttpActionResult AgregarCliente(string nombre)
        {
            var command =
                new AgregarCliente(
                    nombre,
                    SequentialGuid.CreateNew(),
                    typeof(ClientesYSaldos).Name);

            this.bus.Send(command);
            return this.Ok();
        }

        [HttpGet]
        [Route("api/cliente/agregarSaldo/{idcliente}/{saldo}")]
        public IHttpActionResult AgregarSaldo(Guid idCliente, int saldo)
        {
            var command =
                new AgregarSaldo(
                    saldo,
                    idCliente,
                    SequentialGuid.CreateNew(),
                    typeof(ClientesYSaldos).Name);

            this.bus.Send(command);
            return this.Ok();
        }

        [HttpGet]
        [Route("api/cliente/quitarSaldo/{idcliente}/{saldo}")]
        public IHttpActionResult QuitarSaldo(Guid idCliente, int saldo)
        {
            var command =
                new QuitarSaldo(
                    saldo,
                    idCliente,
                    SequentialGuid.CreateNew(),
                    typeof(ClientesYSaldos).Name);

            this.bus.Send(command);
            return this.Ok();
        }
    }
}
