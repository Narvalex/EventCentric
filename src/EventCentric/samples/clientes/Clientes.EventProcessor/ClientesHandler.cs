using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace Clientes.EventProcessor
{
    public class ClientesHandler : EventProcessor<Clientes>,
        IEventHandler<SolicitudNuevoClienteRecibida>
    {
        public ClientesHandler(IBus bus, ILogger log, IEventStore<Clientes> store)
            : base(bus, log, store)
        { }

        public void Handle(SolicitudNuevoClienteRecibida incomingEvent)
        {
            base.CreateNewStream(incomingEvent.IdCliente, incomingEvent);
        }
    }
}
