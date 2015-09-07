using EventCentric.Queueing;
using System;

namespace Clientes.Events
{
    public class ClienteAbonoPorSaldo : QueuedEvent
    {
        public ClienteAbonoPorSaldo(int monto, Guid idCliente, Guid transactionId)
            : base(idCliente, transactionId)
        {
            this.IdCliente = idCliente;
            this.Monto = monto;
        }

        public Guid IdCliente { get; private set; }
        public int Monto { get; private set; }
    }
}
