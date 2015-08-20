using EventCentric.Processing;
using System;

namespace Clientes.Commands
{
    public class RegistrarNuevoCliente : ClientCommand
    {
        public RegistrarNuevoCliente(string nombre, Guid eventId, string streamType)
            : base(eventId, streamType)
        {
            this.Nombre = nombre;
        }

        public string Nombre { get; private set; }
    }
}
