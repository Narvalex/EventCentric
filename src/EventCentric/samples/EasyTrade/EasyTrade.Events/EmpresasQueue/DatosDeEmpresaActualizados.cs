using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.Queueing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class DatosDeEmpresaActualizados : QueuedEvent
    {
        public DatosDeEmpresaActualizados(Guid streamId, Guid transactionId,
            Empresa empresa, DateTime fechaActualizacion)
            : base(streamId, transactionId)
        {
            this.Empresa = empresa;
            this.FechaActualizacion = fechaActualizacion;
        }

        public Empresa Empresa { get; private set; }
        public DateTime FechaActualizacion { get; private set; }
    }
}
