using Clientes.Events;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Processing;
using System;

namespace Clientes.ReadModel
{
    public class ClientesSubscriber : EventProcessor<ClientesDenormalizer>,
        IEventHandler<CuentaCreadaANuevoCliente>,
        IEventHandler<SolicitudNuevoClienteRecibida>
    {
        public ClientesSubscriber(IBus bus, ILogger log, IEventStore<ClientesDenormalizer> store)
            : base(bus, log, store)
        { }

        public void Handle(SolicitudNuevoClienteRecibida incomingEvent)
        {
            base.CreateNewStreamIfNotExists(Guid.Empty, incomingEvent);
        }

        public void Handle(CuentaCreadaANuevoCliente incomingEvent)
        {
            base.CreateNewStreamIfNotExists(Guid.Empty, incomingEvent);
        }
    }
}
