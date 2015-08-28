using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;
using EventCentric.Transport;

namespace Clientes.EventProcessor
{
    public class ClientesHandler : EventProcessor<Clientes>,
        IEventHandler<SolicitudNuevoClienteRecibida>
    {
        public ClientesHandler(IBus bus, IEventStore<Clientes> store)
            : base(bus, store)
        { }

        public void Handle(IncomingEvent<SolicitudNuevoClienteRecibida> incomingEvent)
        {
            base.CreateNewStream(incomingEvent.Event.IdCliente, incomingEvent);
        }
    }
}
