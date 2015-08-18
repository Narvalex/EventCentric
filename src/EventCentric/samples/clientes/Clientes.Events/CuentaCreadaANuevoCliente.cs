using EventCentric.EventSourcing;
using System;

namespace Clientes.Events
{
    public class CuentaCreadaANuevoCliente : Event
    {
        public Guid IdCliente { get; set; }
        public string Nombre { get; set; }
    }
}
