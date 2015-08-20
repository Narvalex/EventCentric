using Clientes.Events;
using EventCentric.Messaging;
using EventCentric.Utils;
using System;

namespace Clientes.Client
{
    public class TelefonicaAdminClient : ITelefonicaAdminClient
    {
        private readonly IEventBus bus;

        public TelefonicaAdminClient(IEventBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.bus = bus;
        }

        public void SolicitudNuevoClienteRecibida(string nombre)
        {
            // Silly validation
            this.bus.Send(new SolicitudNuevoClienteRecibida(nombre));
        }

        public void ClienteAbonoPorSaldo(Guid idCliente, int montoAbonado)
        {
            // Silly vallidation
            this.bus.Send(new ClienteAbonoPorSaldo(montoAbonado, idCliente));
        }

        public void ClienteGastoSaldo(Guid idCliente, int saldoGastado)
        {
            // Silly Validation
            this.bus.Send(new ClienteGastoSaldo(saldoGastado, idCliente));
        }
    }
}
