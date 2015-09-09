using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.Queueing;
using System;

namespace EasyTrade.Events
{
    public class NuevaEmpresaRegistrada : QueuedEvent
    {
        public NuevaEmpresaRegistrada(Guid streamId, Guid transactionId,
            Empresa empresa, DateTime fechaRegistro)
            : base(streamId, transactionId)
        {
            this.Empresa = empresa;
            this.FechaRegistro = fechaRegistro;
        }

        public Empresa Empresa { get; private set; }
        public DateTime FechaRegistro { get; private set; }
    }
}
