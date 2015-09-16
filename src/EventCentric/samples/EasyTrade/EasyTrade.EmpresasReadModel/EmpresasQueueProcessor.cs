﻿using EasyTrade.Events;
using EasyTrade.Events.EmpresasQueue;
using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Processing;

namespace EasyTrade.EmpresasReadModel
{
    public class EmpresasQueueProcessor : EventProcessor<EmpresasQueueDenormalizer>, IEmpresasQueueSubscriber
    {
        public EmpresasQueueProcessor(IBus bus, ILogger log, IEventStore<EmpresasQueueDenormalizer> store)
            : base(bus, log, store)
        { }

        public void Handle(DatosDeEmpresaActualizados incomingEvent)
        {
            this.Handle(incomingEvent.Empresa.IdEmpresa, incomingEvent);
        }

        public void Handle(EmpresaReactivada incomingEvent)
        {
            this.Handle(incomingEvent.IdEmpresa, incomingEvent);
        }

        public void Handle(EmpresaDesactivada incomingEvent)
        {
            this.Handle(incomingEvent.IdEmpresa, incomingEvent);
        }

        public void Handle(NuevaEmpresaRegistrada incomingEvent)
        {
            this.CreateNewStream(incomingEvent.Empresa.IdEmpresa, incomingEvent);
        }
    }
}