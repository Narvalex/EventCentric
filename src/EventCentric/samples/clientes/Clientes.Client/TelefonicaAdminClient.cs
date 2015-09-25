using Clientes.Events;
using EventCentric.Queueing;
using EventCentric.Utils;
using System;

namespace Clientes.Client
{
    public class TelefonicaAdminClient : ITelefonicaAdminClient
    {
        private readonly IGuidProvider guidProvider;
        private readonly IEventQueue bus;

        public TelefonicaAdminClient(IEventQueue bus)
        {
            Ensure.NotNull(bus, "bus");

            this.guidProvider = new SequentialGuid();
            this.bus = bus;
        }

        public void SolicitudNuevoClienteRecibida(string nombre)
        {
            // Silly validation
            this.bus.Enqueue(new SolicitudNuevoClienteRecibida(nombre, this.guidProvider.NewGuid(), this.guidProvider.NewGuid()));
        }

        public void ClienteAbonoPorSaldo(Guid idCliente, int montoAbonado)
        {
            // Silly vallidation
            this.bus.Enqueue(new ClienteAbonoPorSaldo(montoAbonado, idCliente, this.guidProvider.NewGuid()));
        }

        public void ClienteGastoSaldo(Guid idCliente, int saldoGastado)
        {
            // Silly Validation
            this.bus.Enqueue(new ClienteGastoSaldo(saldoGastado, idCliente, this.guidProvider.NewGuid()));
        }
    }
}
