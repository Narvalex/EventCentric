using EventCentric.EventSourcing;
using System;

namespace Clientes.Events
{
    public class ClienteRegistrado : Event
    {
        public Guid IdCliente { get; set; }
        public string Nombre { get; set; }
    }
}
