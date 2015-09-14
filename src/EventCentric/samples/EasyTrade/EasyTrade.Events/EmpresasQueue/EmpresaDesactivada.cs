using EventCentric.Queueing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class EmpresaDesactivada : QueuedEvent
    {
        public EmpresaDesactivada(Guid streamId, Guid transactionId,
            Guid IdEmpresa)
            : base(streamId, transactionId)
        {
            this.IdEmpresa = IdEmpresa;
        }

        public Guid IdEmpresa { get; private set; }
    }
}

