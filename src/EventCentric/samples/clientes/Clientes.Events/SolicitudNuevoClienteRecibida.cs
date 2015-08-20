using EventCentric.Queueing;

namespace Clientes.Events
{
    public class SolicitudNuevoClienteRecibida : QueuedEvent
    {
        public SolicitudNuevoClienteRecibida(string nombre)
        {
            this.Nombre = nombre;
        }

        public string Nombre { get; private set; }
    }
}
