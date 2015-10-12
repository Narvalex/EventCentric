using EasyTrade.Events.EmpresasQueue.DTOs;
using EventCentric.EventSourcing;
using System;

namespace EasyTrade.Events
{
    public class NuevaEmpresaRegistrada : Event
    {
        public NuevaEmpresaRegistrada(
            Empresa empresa, DateTime fechaRegistro)
        {
            this.Empresa = empresa;
            this.FechaRegistro = fechaRegistro;
        }

        public Empresa Empresa { get; private set; }
        public DateTime FechaRegistro { get; private set; }
    }
}
