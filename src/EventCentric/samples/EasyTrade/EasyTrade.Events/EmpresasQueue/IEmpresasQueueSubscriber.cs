﻿using EventCentric.Processing;

namespace EasyTrade.Events.EmpresasQueue
{
    public interface IEmpresasQueueSubscriber :
        IEventHandler<NuevaEmpresaRegistrada>,
        IEventHandler<EmpresaDesactivada>,
        IEventHandler<EmpresaReactivada>,
        IEventHandler<DatosDeEmpresaActualizados>
    { }
}