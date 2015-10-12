using EventCentric.EventSourcing;
using System;

namespace EasyTrade.Events.EmpresasQueue
{
    public class EmpresaReactivada : Event
    {
        public EmpresaReactivada(Guid IdEmpresa)
        {
            this.IdEmpresa = IdEmpresa;
        }

        public Guid IdEmpresa { get; private set; }
    }
}
