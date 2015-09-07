using Clientes.Events;
using EventCentric.Queueing;
using EventCentric.Utils;
using System;

namespace Clientes.Client
{
    public class TelefonicaAdminClient : ITelefonicaAdminClient
    {
        private readonly IGuidProvider guidProvider;
        private readonly IEventBus bus;

        public TelefonicaAdminClient(IEventBus bus)
        {
            Ensure.NotNull(bus, "bus");

            this.guidProvider = new SequentialGuid();
            this.bus = bus;
        }

        public void SolicitudNuevoClienteRecibida(string nombre)
        {
            // Silly validation
            this.bus.Send(new SolicitudNuevoClienteRecibida(nombre, this.guidProvider.NewGuid));
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
