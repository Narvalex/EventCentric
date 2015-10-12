using EventCentric.EventSourcing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class EmpresaDesactivada : Event
    {
        public EmpresaDesactivada(Guid IdEmpresa)
        {
            this.IdEmpresa = IdEmpresa;
        }

        public Guid IdEmpresa { get; private set; }
    }
}

