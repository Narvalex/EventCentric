using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.Queueing;
using System;

namespace EasyTrade.Events
{
    public class NuevaEmpresaRegistrada : QueuedEvent
    {
        public NuevaEmpresaRegistrada(Guid streamId, Guid transactionId,
            Empresa empresa)
            : base(streamId, transactionId)
        {
            this.Empresa = empresa;
        }

        public Empresa Empresa { get; private set; }
    }
}
