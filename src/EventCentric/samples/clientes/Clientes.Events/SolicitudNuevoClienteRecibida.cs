using EventCentric.Queueing;
using System;

namespace Clientes.Events
{
    public class SolicitudNuevoClienteRecibida : QueuedEvent
    {
        public SolicitudNuevoClienteRecibida(string nombre, Guid idCliente, Guid transactionId)
            : base(idCliente, transactionId)
        {
            this.Nombre = nombre;
            this.IdCliente = idCliente;
        }

        public Guid IdCliente { get; private set; }
        public string Nombre { get; private set; }
    }
}
