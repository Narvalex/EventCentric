using EventCentric.Queueing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class EmpresaReactivada : QueuedEvent
    {
        public EmpresaReactivada(Guid streamId, Guid transactionId,
            Guid IdEmpresa)
            : base(streamId, transactionId)
        {
            this.IdEmpresa = IdEmpresa;
        }

        public Guid IdEmpresa { get; private set; }
    }
}
