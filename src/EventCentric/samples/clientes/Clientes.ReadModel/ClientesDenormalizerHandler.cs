using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Messaging;
using EventCentric.Processing;
using System;

namespace Clientes.ReadModel
{
    public class ClientesDenormalizerHandler : EventProcessor<ClientesDenormalizer>,
        IEventHandler<CuentaCreadaANuevoCliente>
    {
        public ClientesDenormalizerHandler(IBus bus, IEventStore<ClientesDenormalizer> store, ISubscriptionInboxWriter subscriptionWriter)
            : base(bus, store, subscriptionWriter)
        { }

        public void Handle(CuentaCreadaANuevoCliente @event)
        {
            base.CreateNewStreamIfNotExists(Guid.Empty, @event);
        }
    }
}
