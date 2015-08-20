using EventCentric.Processing;
using System;

namespace Clientes.Commands
{
    public class QuitarSaldo : ClientCommand
    {
        public QuitarSaldo(int monto, Guid idCliente, Guid eventId, string streamType)
            : base(eventId, streamType)
        {
            this.IdCliente = idCliente;
            this.Monto = monto;
        }

        public Guid IdCliente { get; private set; }
        public int Monto { get; private set; }
    }
}
