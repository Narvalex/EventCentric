using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace Clientes.EventProcessor
{
    public class ClientesHandler : EventProcessor<Clientes>,
        IEventHandler<SolicitudNuevoClienteRecibida>
    {
        public ClientesHandler(IBus bus, IEventStore<Clientes> store)
            : base(bus, store)
        { }

        public void Handle(SolicitudNuevoClienteRecibida incomingEvent)
        {
            base.CreateNewStream(incomingEvent.IdCliente, incomingEvent);
        }
    }
}
