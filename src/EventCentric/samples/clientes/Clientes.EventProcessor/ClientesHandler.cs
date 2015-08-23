using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace Clientes.EventProcessor
{
    public class ClientesHandler : EventProcessor<Clientes>,
        IEventHandler<SolicitudNuevoClienteRecibida>
    {
        public ClientesHandler(IBus bus, IEventStore<Clientes> store, ISubscriptionInboxWriter subscriptionWriter)
            : base(bus, store, subscriptionWriter)
        { }

        public void Handle(SolicitudNuevoClienteRecibida @event)
        {
            base.CreateNewStream(@event.IdCliente, @event);
        }
    }
}
