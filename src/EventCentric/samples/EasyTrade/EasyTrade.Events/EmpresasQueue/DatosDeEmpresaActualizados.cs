using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.EventSourcing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class DatosDeEmpresaActualizados : Event
    {
        public DatosDeEmpresaActualizados(
            Empresa empresa, DateTime fechaActualizacion)
        {
            this.Empresa = empresa;
            this.FechaActualizacion = fechaActualizacion;
        }

        public Empresa Empresa { get; private set; }
        public DateTime FechaActualizacion { get; private set; }
    }
}
