using EventCentric.Processing;
using System;

namespace Clientes.CommandProcessor.Commands
{
    public class AgregarCliente : ClientCommand
    {
        public AgregarCliente(string nombre, Guid eventId, string streamType)
            : base(eventId, streamType)
        {
            this.Nombre = nombre;
        }

        public string Nombre { get; private set; }
    }
}
