using EasyTrade.Events;
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

        public void Receive(DatosDeEmpresaActualizados @event)
        {
            this.Process(@event.Empresa.IdEmpresa, @event);
        }

        public void Receive(EmpresaReactivada @event)
        {
            this.Process(@event.IdEmpresa, @event);
        }

        public void Receive(EmpresaDesactivada @event)
        {
            this.Process(@event.IdEmpresa, @event);
        }

        public void Receive(NuevaEmpresaRegistrada @event)
        {
            this.CreateNewStreamAndProcess(@event.Empresa.IdEmpresa, @event);
        }
    }
}
